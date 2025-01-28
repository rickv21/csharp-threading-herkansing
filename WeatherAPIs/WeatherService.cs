using WeatherApp.Models;
using WeatherApp.Utils;
using System.Reflection;
using System.Diagnostics;

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
        /// Gets the weather data for a day and returns the weather data for each hour as a list.
        /// </summary>
        /// <param name="day">The day that will be passed to the API, only the day of the DateTime object will be looked at.</param>
        /// <param name="location">The location as a string that will be passed to the API.</param>
        /// <param name="simulate">If the request should be simulated instead of sending a actual request, defaults to false if not set.</param>
        /// <returns>A APIResponse Task that should be awaited. The API response will contain the data is Success is true, otherwise data will be true and ErrorMessage will be set.</returns>
        public abstract Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location, bool simulate = false);

        /// <summary>
        /// Gets the weather data for the current week and returns the weather data for each day as a list.
        /// </summary>
        /// <param name="location">The location as a string that will be passed to the API.</param>
        /// <param name="simulate">If the request should be simulated instead of sending a actual request, defaults to false if not set.</param>
        /// <returns>A APIResponse Task that should be awaited. The API response will contain the data is Success is true, otherwise data will be true and ErrorMessage will be set.</returns>
        public abstract Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(LocationModel location, bool simulate = false); //TODO: Maybe we could pass a week number if we later want to check more weeks.

        protected bool GetEnabled()
        {
            JsonFileManager jsonManager = new JsonFileManager();

            // Retrieve the data
            var data = jsonManager.GetData("status", Name, "enabled");

            if (data != null && data is bool isEnabled) {
                return isEnabled; 
            }
            else
            {
                Debug.WriteLine("Data is null, empty or not a valid boolean");
                return true;
            }
        }

        /// <summary>
        /// Converts weather info from an API to a WeatherCondition enum.
        /// </summary>
        /// <param name="data">The weather info. Is a object because some API's use codes and some names.</param>
        /// <returns>The WeatherCondition.</returns>
        protected abstract WeatherCondition CalculateWeatherCondition(object data);
    }
}
