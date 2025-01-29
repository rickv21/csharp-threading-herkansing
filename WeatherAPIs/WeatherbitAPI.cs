using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using WeatherApp.Models;
using System.Globalization;

namespace WeatherApp.WeatherAPIs
{
    internal class WeatherbitAPI : WeatherService
    {
        public WeatherbitAPI() : base("Weatherbit", "http://api.weatherbit.io/v2.0/forecast/daily", 50, -1)
        {
        }

        //not possible with free weatherbit subscription, sends empty data 
        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location, bool simulate = false)
        {
            Debug.WriteLine($"Requesting weather data for {Name} on {day:yyyy-MM-dd}.");

            // sends empty data, to prevent throwing exception
            return new APIResponse<List<WeatherDataModel>>
            {
                Success = true,
                Source = Name,
                Data = new List<WeatherDataModel>() // Lege lijst
            };
        }


        /// <summary>
        /// Requests forecast data for a week
        /// </summary>
        /// <param name="location"></param>
        /// <param name="simulate"></param>
        /// <returns>7 days of weather data</returns>
        /// <exception cref="Exception"></exception>
        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(LocationModel location, bool simulate = false)
        {
            Debug.WriteLine($"Requesting week data for {Name}.");
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
            if (simulate)
            {
                responseBody = GetTestJSON("weatherbit_week_test.json");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string latitude = location.Latitude.ToString(CultureInfo.InvariantCulture);
                    string longitude = location.Longitude.ToString(CultureInfo.InvariantCulture);

                    string url = $"{_baseURL}?key={_apiKey}&lat={latitude}&lon={longitude}&days=7";

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("Error: " + responseBody);

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
            }

            Debug.WriteLine(responseBody);
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
        /// Calculates the weathercondition
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Weathercondition</returns>
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
