using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Models;
using Newtonsoft.Json.Linq;
using DotNetEnv;

namespace WeatherApp.WeatherAPIs
{
    public abstract class WeatherService
    {
        protected string _apiKey;
        protected string _baseURL;
        public string Name {  get; private set; }
        public ServiceRequestLimit RequestLimitDay { get; private set; }
        public ServiceRequestLimit RequestLimitMonth { get; private set; }

        public WeatherService(string name, string baseURL, int requestLimitDay, int requestLimitMonth)
        {
            Name = name;
            _baseURL = baseURL;
            _apiKey = GetAPIKeyFromEnv();

            GetCurrentRequestsFromFile(requestLimitDay, requestLimitMonth);
            if (RequestLimitDay == null || RequestLimitMonth == null)
            {
                throw new InvalidOperationException("Failed to initialize request limits.");
            }
        }

        public abstract Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, string Location);
        public abstract Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(string Location);

        protected string GetAPIKeyFromEnv()
        {
            string baseDirectory = AppContext.BaseDirectory;
            string envPath = Path.Combine(baseDirectory, ".env");

           DotNetEnv.Env.Load(envPath);
            string apiKey = Environment.GetEnvironmentVariable(Name.ToUpper() + "_API_KEY") ?? throw new InvalidOperationException("API key not found in environment variables");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("API key not found in environment variables.");
            }
            return apiKey;
        }

        protected void GetCurrentRequestsFromFile(int requestLimitDay, int requestLimitMonth)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "weatherAppData.json");
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string currentMonth = DateTime.Now.ToString("yyyy-MM");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                JObject requestCountsWrapper = JObject.Parse(json);
                if (requestCountsWrapper["requestCounts"] is JObject requestCounts && requestCounts[Name] is JObject counts)
                {
                    if (counts["requestsDay"]?["date"]?.ToString() != currentDate)
                    {
                        counts["requestsDay"] = new JObject
                    {
                            { "count", 0 },
                            { "date", currentDate }
                    };
                    }

                    if (counts["requestsMonth"]?["month"]?.ToString() != currentMonth)
                    {
                        counts["requestsMonth"] = new JObject
                    {
                        { "count", 0 },
                        { "month", currentMonth }
                    };
                        }

                    // Update the request limits here, based on updated counts from the file
                    RequestLimitDay = new ServiceRequestLimit(requestLimitDay, (int)counts["requestsDay"]?["count"]!);
                    RequestLimitMonth = new ServiceRequestLimit(requestLimitMonth, (int)counts["requestsMonth"]?["count"]!);

                    // Save the potentially reset counts
                    SaveRequestCountsToFile();
                }
            }
            else
            {
                RequestLimitDay = new ServiceRequestLimit(-1, 0);
                RequestLimitMonth = new ServiceRequestLimit(-1, 0);

                SaveRequestCountsToFile();
            }
        }


        protected bool HasReachedRequestLimit()
        {
            if (RequestLimitDay.HasReachedLimit() || RequestLimitMonth.HasReachedLimit())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void CountRequest()
        {
            RequestLimitDay.CountRequest();
            RequestLimitMonth.CountRequest();
            SaveRequestCountsToFile();
        }


        protected void SaveRequestCountsToFile()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "weatherAppData.json");
            JObject requestCountsWrapper;

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                requestCountsWrapper = JObject.Parse(json);
            }
            else
            {
                requestCountsWrapper = new JObject();
            }

            if (requestCountsWrapper["requestCounts"] is not JObject requestCounts)
            {
                requestCounts = new JObject();
                requestCountsWrapper["requestCounts"] = requestCounts;
            }

            if (requestCounts[Name] is not JObject serviceCounts)
            {
                serviceCounts = new JObject();
                requestCounts[Name] = serviceCounts;
            }

            serviceCounts["requestsDay"] = new JObject
            {
                { "count", RequestLimitDay.CurrentRequestCount },
                { "date", DateTime.Now.ToString("yyyy-MM-dd") }
            };

            serviceCounts["requestsMonth"] = new JObject
            {
                { "count", RequestLimitMonth.CurrentRequestCount },
                { "month", DateTime.Now.ToString("yyyy-MM") }
            };

            string updatedJson = requestCountsWrapper.ToString();
            File.WriteAllText(filePath, updatedJson);
        }




    }
}
