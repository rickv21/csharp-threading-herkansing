﻿using WeatherApp.Models;
using Newtonsoft.Json.Linq;

namespace WeatherApp.WeatherAPIs
{
    /// <summary>
    /// Base class for API WeatherServices.
    /// </summary>
    public abstract class WeatherService
    {
        /// <summary>
        /// The API key of the API, is set by GetAPIKeyFromEnv()
        /// </summary>
        protected string _apiKey;

        /// <summary>
        /// The baseURL of the API that is used as a base to send requests.
        /// </summary>
        protected string _baseURL;

        /// <summary>
        /// The name of the API.
        /// Is used for getting the API key from .env and in the settings view.
        /// </summary>
        public string Name {  get; private set; }

        /// <summary>
        /// The request limit and current request data for the current day.
        /// </summary>
        public ServiceRequestLimit RequestLimitDay { get; private set; }

        /// <summary>
        /// The request limit and current request data for the current month.
        /// </summary>
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

        /// <summary>
        /// Gets the weather data for a day and returns the weather data for each hour as a list.
        /// </summary>
        /// <param name="day">The day that will be passed to the API, only the day of the DateTime object will be looked at.</param>
        /// <param name="location">The location as a string that will be passed to the API.</param>
        /// <param name="simulate">If the request should be simulated instead of sending a actual request, defaults to false if not set.</param>
        /// <returns>A APIResponse Task that should be awaited. The API response will contain the data is Success is true, otherwise data will be true and ErrorMessage will be set.</returns>
        public abstract Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, string location, bool simulate = false);

        /// <summary>
        /// Gets the weather data for the current week and returns the weather data for each day as a list.
        /// </summary>
        /// <param name="location">The location as a string that will be passed to the API.</param>
        /// <param name="simulate">If the request should be simulated instead of sending a actual request, defaults to false if not set.</param>
        /// <returns>A APIResponse Task that should be awaited. The API response will contain the data is Success is true, otherwise data will be true and ErrorMessage will be set.</returns>
        public abstract Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(string location, bool simulate = false); //TODO: Maybe we could pass a week number if we later want to check more weeks.


        /// <summary>
        /// Loads the API key for this API from the .env file.
        /// This file is in the root of the project during development.
        /// It looks for the Name of the WeatherService in uppercase and appends _API_KEY to it.
        /// To in the case of a WeatherService called Test, it will be looking for: TEST_API_KEY.
        /// </summary>
        /// <returns>The API key.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the API key is not found.</exception>
        protected string GetAPIKeyFromEnv()
        {
            string baseDirectory = AppContext.BaseDirectory;
            string envPath = Path.Combine(baseDirectory, ".env");

            DotNetEnv.Env.Load(envPath);
            string apiKey = Environment.GetEnvironmentVariable(Name.ToUpper().Replace(" ", "_") + "_API_KEY") ?? throw new InvalidOperationException("API key not found in environment variables");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("API key not found in environment variables.");
            }
            return apiKey;
        }

        /// <summary>
        /// Looks for a weatherAppData.json in the Documents folder of the user, and if found loads the request values from there.
        /// This contains the current requests for the day and month and stores the date of the last request.
        /// This is so the requests counters are persisted across application restarts.
        /// This function sets the RequestLimitDay and RequestLimitMonth variables.
        /// </summary>
        /// <param name="requestLimitDay">The request limit per day.</param>
        /// <param name="requestLimitMonth">The request limit per month.</param>
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
                else
                {
                    RequestLimitDay = new ServiceRequestLimit(requestLimitDay, 0);
                    RequestLimitMonth = new ServiceRequestLimit(requestLimitMonth, 0);

                    SaveRequestCountsToFile();
                }
            }
            else
            {
                RequestLimitDay = new ServiceRequestLimit(requestLimitDay, 0);
                RequestLimitMonth = new ServiceRequestLimit(requestLimitMonth, 0);

                SaveRequestCountsToFile();
            }
        }

        /// <summary>
        /// Saves the current RequestLimitDay and RequestLimitMonth values and date to the weatherAppData.json file in the Documents folder of the user.
        /// </summary>
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


        /// <summary>
        /// Checks if the API has reached one of the request limits.
        /// </summary>
        /// <returns>True if a limit has been reached, otherwise false.</returns>
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

        /// <summary>
        /// Increments the request values by 1 and calls SaveRequestCountsToFile().
        /// </summary>
        protected void CountRequest()
        {
            RequestLimitDay.CountRequest();
            RequestLimitMonth.CountRequest();
            SaveRequestCountsToFile();
        }

        /// <summary>
        /// Converts weather info from an API to a WeatherCondition enum.
        /// </summary>
        /// <param name="data">The weather info. Is a object because some API's use codes and some names.</param>
        /// <returns>The WeatherCondition.</returns>
        protected abstract WeatherCondition CalculateWeatherCondition(object data);

        /// <summary>
        /// Returns a example JSON query result to use for testing.
        /// </summary>
        /// <returns>The test data.</returns>
        protected abstract string GetTestJSON();
    }
}
