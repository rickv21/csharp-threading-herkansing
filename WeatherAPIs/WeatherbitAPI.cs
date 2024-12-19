using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    internal class WeatherbitAPI : WeatherService
    {
        public WeatherbitAPI() : base("Weatherbit", "http://api.weatherbit.io/v2.0/forecast/daily", 50, -1)
        {
        }

        //not possible with free weatherbit subscription
        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location, bool simulate = false)
        {
            Debug.WriteLine($"Requesting weather data for {Name} on {day:yyyy-MM-dd}.");

            // Expliciet lege data teruggeven
            return new APIResponse<List<WeatherDataModel>>
            {
                Success = true,
                ErrorMessage = "No weather data available for the requested date.",
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
                    Data = null
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
                    string url = $"{_baseURL}?key={_apiKey}&city={location.Name}&days=7";  // 7 days forecast
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
                            Data = null
                        };
                    }
                    CountRequest();
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            Debug.WriteLine(responseBody);
            JObject weatherResponse = JObject.Parse(responseBody);

            var forecastDays = weatherResponse["data"] ?? throw new Exception("Missing forecast data in API response.");
            var weatherData = new List<WeatherDataModel>();

            foreach (var day in forecastDays)
            {
                var condition = CalculateWeatherCondition(day["weather"]?["description"]?.ToString());
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
                ErrorMessage = null,
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
            string condition = ((string)data).Trim().ToLower(); //Deletes space and lowercase

            switch (condition)
            {
                case "clear sky":
                    return WeatherCondition.CLEAR;
                case "few clouds":
                case "scattered clouds":
                    return WeatherCondition.PARTLY_CLOUDY;
                case "broken clouds":
                case "overcast clouds":
                    return WeatherCondition.CLOUDY;
                case "light drizzle":
                case "drizzle":
                case "heavy drizzle":
                case "freezing drizzle":
                case "heavy freezing drizzle":
                case "patchy light drizzle":
                    return WeatherCondition.DRIZZLE;
                case "light rain":
                case "shower rain":
                case "moderate rain":
                case "heavy rain":
                case "light shower rain":
                case "moderate or heavy rain shower":
                case "torrential rain shower":
                case "patchy light rain with thunder":
                case "moderate or heavy rain with thunder":
                    return WeatherCondition.RAIN;
                case "snow":
                case "light snow":
                case "heavy snow":
                case "snow shower":
                case "heavy snow shower":
                case "flurries":
                case "patchy light snow":
                case "moderate snow":
                case "patchy heavy snow":
                    return WeatherCondition.SNOW;
                case "thunderstorm with light rain":
                case "thunderstorm with rain":
                case "thunderstorm with heavy rain":
                case "thunderstorm with light drizzle":
                case "thunderstorm with drizzle":
                case "thunderstorm with heavy drizzle":
                case "thunderstorm with hail":
                case "patchy light snow with thunder":
                case "moderate or heavy snow with thunder":
                    return WeatherCondition.THUNDERSTORM;
                case "hail":
                case "light snow with hail":
                case "moderate or heavy snow with hail":
                case "light showers of ice pellets":
                case "moderate or heavy showers of ice pellets":
                    return WeatherCondition.HAIL;
                case "mist":
                    return WeatherCondition.MIST;
                case "smoke":
                    return WeatherCondition.SMOKE;
                case "haze":
                    return WeatherCondition.HAZE;
                case "sand/dust":
                    return WeatherCondition.SAND;
                case "fog":
                case "freezing fog":
                    return WeatherCondition.FOG;
                case "patchy snow possible":
                case "patchy sleet possible":
                case "patchy freezing drizzle possible":
                case "thundery outbreaks possible":
                case "blowing snow":
                case "blizzard":
                    return WeatherCondition.SNOW;
                case "unknown precipitation":
                default:
                    return WeatherCondition.UNKNOWN;
            }
        }
    }
}
