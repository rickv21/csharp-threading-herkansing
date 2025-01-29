using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeatherApp.Models;
using System.Globalization;

namespace WeatherApp.WeatherAPIs
{
    public class WeatherbitAPI : WeatherService
    {
        public WeatherbitAPI() : base("Weatherbit", "http://api.weatherbit.io/v2.0/forecast/daily", 50, -1)
        {
        }

        /// <summary>
        /// Get weather data of a location
        /// </summary>
        /// <param name="day">The day of which the weather should be retrieved</param>
        /// <param name="location">The location of which the weather should be retrieved</param>
        /// <returns>An APIResponse with a list of WeatherDataModels</returns>
        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location)
        {
            // Sends empty data to prevent throwing exception
            return new APIResponse<List<WeatherDataModel>>
            {
                Success = true,
                Source = Name,
                Data = []
            };
        }

        /// <summary>
        /// Get weatherdata of a full week
        /// </summary>
        /// <param name="location">The location of which the weather should be retrieved</param>
        /// <returns>An APIResponse with a list of WeatherDataModels</returns>
        /// <exception cref="Exception">An exception for when the processing of weatherdata fails</exception>
        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(LocationModel location)
        {
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached. Reset your request count or check the API settings.",
                    Source = Name
                };
            }

            string responseBody;

            using (HttpClient client = new())
            {
                string latitude = location.Latitude.ToString(CultureInfo.InvariantCulture);
                string longitude = location.Longitude.ToString(CultureInfo.InvariantCulture);

                string url = $"{_baseURL}?key={_apiKey}&lat={latitude}&lon={longitude}&days=7";

                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    responseBody = await response.Content.ReadAsStringAsync();

                    var errorResponse = JObject.Parse(responseBody);
                    string errorCode = errorResponse["error"]?["code"]?.ToString() ?? "Unknown Code";
                    string errorMessage = errorResponse["error"]?["message"]?.ToString() ?? "Unknown Error";
                    return new APIResponse<List<WeatherDataModel>>
                    {
                        Success = false,
                        ErrorMessage = $"{errorCode} - {errorMessage}",
                        Source = Name
                    };
                }
                responseBody = await response.Content.ReadAsStringAsync();
            }

            JObject weatherResponse = JObject.Parse(responseBody);

            var forecastDays = weatherResponse["data"] ?? throw new Exception("Missing forecast data in API response.");
            var weatherData = new List<WeatherDataModel>();

            foreach (var day in forecastDays)
            {
                int conditionCode = (int)(day["weather"]?["code"] ?? throw new Exception("Missing condition code."));
                var condition = CalculateWeatherCondition(conditionCode);

                var forecastDate = DateTime.Parse(day["datetime"]?.ToString()!);

                double humidity = day["rh"] != null ? (double)day["rh"] : 0;

                weatherData.Add(new WeatherDataModel(
                    condition,
                    forecastDate,
                    minTemperature: (double)day["min_temp"]!,
                    maxTemperature: (double)day["max_temp"]!,
                    humidity: humidity
                ));
            }

            return new APIResponse<List<WeatherDataModel>>
            {
                Success = true,
                Source = Name,
                Data = weatherData
            };
        }

        /// <summary>
        /// Get the weathercondition based on the ID of the several known weatherconditions
        /// </summary>
        /// <param name="data">The ID of a weathercondition</param>
        /// <returns>The weathercondition that's connected to the ID</returns>
        protected override WeatherCondition CalculateWeatherCondition(object data)
        {
            int id = (int)data;

            return id switch
            {
                800 => WeatherCondition.CLEAR, // Clear sky
                801 or 802 => WeatherCondition.PARTLY_CLOUDY, // Few clouds, scattered clouds
                803 or 804 => WeatherCondition.CLOUDY, // Broken clouds, overcast clouds
                700 => WeatherCondition.MIST, // Mist
                711 => WeatherCondition.SMOKE, // Smoke
                721 => WeatherCondition.HAZE, // Haze
                731 => WeatherCondition.SAND, // Sand
                741 or 751 => WeatherCondition.FOG, // Fog
                >= 600 and <= 623 => WeatherCondition.SNOW, // Snow conditions
                >= 500 and <= 522 => WeatherCondition.RAIN, // Rain conditions
                >= 300 and <= 302 => WeatherCondition.DRIZZLE, // Drizzle conditions
                >= 200 and <= 233 => WeatherCondition.THUNDERSTORM, // Thunderstorm conditions
                900 => WeatherCondition.UNKNOWN, // Unknown condition
                _ => WeatherCondition.UNKNOWN // Default case
            };
        }
    }
}
