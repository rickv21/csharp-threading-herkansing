using WeatherApp.Models;
using WeatherApp.Utils;

namespace WeatherApp.WeatherAPIs
{
    /// <summary>
    /// Base class for API WeatherServices.
    /// </summary>
    public abstract class WeatherService : APIService
    {
        /// <summary>
        /// If this API is enabled and should be used.
        /// </summary>
        public bool IsEnabled { get; set;}

        public WeatherService(string name, string baseURL, int requestLimitDay, int requestLimitMonth) : base(name, baseURL, requestLimitDay, requestLimitMonth)
        {
            IsEnabled = GetEnabled();
        }

        /// <summary>
        /// Get weather data of a location
        /// </summary>
        /// <param name="day">The day of which the weather should be retrieved</param>
        /// <param name="location">The location of which the weather should be retrieved</param>
        /// <returns>An APIResponse with a list of WeatherDataModels</returns>
        public abstract Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location);

        /// <summary>
        /// Get weatherdata of a full week
        /// </summary>
        /// <param name="location">The location of which the weather should be retrieved</param>
        /// <returns>An APIResponse with a list of WeatherDataModels</returns>
        /// <exception cref="Exception">An exception for when the processing of weatherdata fails</exception>
        public abstract Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(LocationModel location);

        protected bool GetEnabled()
        {
            JsonFileManager jsonManager = new();

            // Retrieve the data
            var data = jsonManager.GetData("status", Name, "enabled");

            if (data != null && data is bool isEnabled) {
                return isEnabled; 
            }
            else
            {
                jsonManager.SetData(true, "status", Name, "enabled");
                return true;
            }
        }

        /// <summary>
        /// Get the weathercondition based on the ID of the several known weatherconditions
        /// </summary>
        /// <param name="data">The ID of a weathercondition</param>
        /// <returns>The weathercondition that's connected to the ID</returns>
        protected abstract WeatherCondition CalculateWeatherCondition(object data);
    }
}
