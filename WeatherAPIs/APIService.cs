using System.Reflection;
using WeatherApp.Utils;

namespace WeatherApp.WeatherAPIs
{
    public abstract class APIService
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
        public string Name { get; private set; }

        /// <summary>
        /// The request limit and current request data for the current day.
        /// </summary>
        public ServiceRequestLimit RequestLimitDay { get; private set; }

        /// <summary>
        /// The request limit and current request data for the current month.
        /// </summary>
        public ServiceRequestLimit RequestLimitMonth { get; private set; }

        public APIService(string name, string baseURL, int requestLimitDay, int requestLimitMonth)
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
            string apiKey = Environment.GetEnvironmentVariable(Name.ToUpper().Replace(" ", "_") + "_API_KEY") ?? throw new InvalidOperationException("API key not found in environment variables.\nPlease check that the correct key/value is set in the .env file.");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("API key not found in environment variables.\nPlease check that the correct key/value is set in the .env file.");
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
            JsonFileManager jsonManager = new JsonFileManager();
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string currentMonth = DateTime.Now.ToString("yyyy-MM");

            int dailyCount = jsonManager.GetData("requestCounts", Name, "requestsDay", "count") as int? ?? 0;
            string? dailyDate = jsonManager.GetData("requestCounts", Name, "requestsDay", "date") as string;

            int monthlyCount = jsonManager.GetData("requestCounts", Name, "requestsMonth", "count") as int? ?? 0;
            string? monthlyDate = jsonManager.GetData("requestCounts", Name, "requestsMonth", "month") as string;

            // Reset counters if dates don't match
            if (dailyDate != currentDate)
            {
                dailyCount = 0;
                jsonManager.SetData(new { count = dailyCount, date = currentDate }, "requestCounts", Name, "requestsDay");
            }

            if (monthlyDate != currentMonth)
            {
                monthlyCount = 0;
                jsonManager.SetData(new { count = monthlyCount, month = currentMonth }, "requestCounts", Name, "requestsMonth");
            }

            RequestLimitDay = new ServiceRequestLimit(requestLimitDay, dailyCount);
            RequestLimitMonth = new ServiceRequestLimit(requestLimitMonth, monthlyCount);

            // Save any changes
            SaveRequestCountsToFile();
        }

        /// <summary>
        /// Saves the current RequestLimitDay and RequestLimitMonth values and date to the weatherAppData.json file in the Documents folder of the user.
        /// </summary>
        protected void SaveRequestCountsToFile()
        {
            JsonFileManager jsonManager = new JsonFileManager();

            // Save daily request count and date
            jsonManager.SetData(
                new { count = RequestLimitDay.CurrentRequestCount, date = DateTime.Now.ToString("yyyy-MM-dd") },
                "requestCounts", Name, "requestsDay"
            );

            // Save monthly request count and month
            jsonManager.SetData(
                new { count = RequestLimitMonth.CurrentRequestCount, month = DateTime.Now.ToString("yyyy-MM") },
                "requestCounts", Name, "requestsMonth"
            );
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
        public void CountRequest()
        {
            RequestLimitDay.CountRequest();
            RequestLimitMonth.CountRequest();
            SaveRequestCountsToFile();
        }

        /// <summary>
        /// Reads an embedded JSON resource file and returns its content as a string.
        /// </summary>
        /// <param name="fileName">The name of the JSON file (include the extension).</param>
        /// <returns>The content of the JSON file as a string.</returns>
        public static string GetTestJSON(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"{assembly.GetName().Name}.TestData.{fileName}";

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException($"Resource '{resourceName}' not found. Ensure the file is embedded as a resource and the folder structure matches.");

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
