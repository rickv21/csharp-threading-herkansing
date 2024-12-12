using Microsoft.Maui.Controls;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    internal class WeatherAPI : WeatherService
    {
        public WeatherAPI() : base("WeatherAPI", "https://api.weatherapi.com/v1/", 1000, -1) // Pas aan op basis van de API-limieten.
        {
        }

        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location, bool simulate = false)
        {
            Debug.WriteLine($"Requesting day data for {Name}.");
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
                responseBody = GetTestJSON("weatherapi_day_test.json");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}forecast.json?key={_apiKey}&q={location.Name}&dt={day:yyyy-MM-dd}";
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
                            Data = null
                        };
                    }
                    CountRequest();
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            Debug.WriteLine(responseBody);
            JObject weatherResponse = JObject.Parse(responseBody);

            // Extract relevant data
            var forecast = weatherResponse["forecast"]?["forecastday"]?.First;
            if (forecast == null) throw new Exception("Missing forecast data in API response.");

            var weatherData = new List<WeatherDataModel>();
            foreach (var hour in forecast["hour"] ?? throw new Exception("Missing hourly data in forecast."))
            {
                var condition = CalculateWeatherCondition(hour["condition"]?["text"]?.ToString());
                var forecastDate = DateTime.Parse(hour["time"]?.ToString()!);
                if (forecastDate.Date != day.Date) continue;

                weatherData.Add(new WeatherDataModel(
                    condition,
                    forecastDate,
                    minTemperature: (double)hour["temp_c"]!,
                    maxTemperature: (double)hour["temp_c"]!,
                    humidity: (double)hour["humidity"]!
                ));
            }

            return new APIResponse<List<WeatherDataModel>>
            {
                Success = true,
                ErrorMessage = null,
                Data = weatherData
            };
        }

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
                responseBody = GetTestJSON("weatherapi_week_test.json");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}forecast.json?key={_apiKey}&q={location.Name}&days=7";
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
                            Data = null
                        };
                    }
                    CountRequest();
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            Debug.WriteLine(responseBody);
            JObject weatherResponse = JObject.Parse(responseBody);

            var forecastDays = weatherResponse["forecast"]?["forecastday"] ?? throw new Exception("Missing forecast data in API response.");
            var weatherData = new List<WeatherDataModel>();

            foreach (var day in forecastDays)
            {
                var condition = CalculateWeatherCondition(day["day"]?["condition"]?["text"]?.ToString());
                var forecastDate = DateTime.Parse(day["date"]?.ToString()!);
                weatherData.Add(new WeatherDataModel(
                    condition,
                    forecastDate,
                    minTemperature: (double)day["day"]?["mintemp_c"]!,
                    maxTemperature: (double)day["day"]?["maxtemp_c"]!,
                    humidity: (double)day["day"]?["avghumidity"]!
                ));
            }

            return new APIResponse<List<WeatherDataModel>>
            {
                Success = true,
                ErrorMessage = null,
                Data = weatherData
            };
        }

        protected override WeatherCondition CalculateWeatherCondition(object data)
        {
            if (data is string strData && int.TryParse(strData, out int id))
            {
                Debug.WriteLine("Weather Icon ID: " + id);

                return id switch
                {
                    113 => WeatherCondition.SUNNY,
                    116 => WeatherCondition.PARTLY_CLOUDY,
                    119 => WeatherCondition.CLOUDY,
                    122 => WeatherCondition.CLOUDY, // Overcast wordt CLOUDY
                    143 => WeatherCondition.MIST,
                    176 => WeatherCondition.DRIZZLE, // Patchy rain wordt DRIZZLE
                    179 => WeatherCondition.SNOW, // Patchy snow wordt SNOW
                    182 => WeatherCondition.SNOW, // Patchy sleet wordt SNOW
                    185 => WeatherCondition.DRIZZLE, // Patchy freezing drizzle wordt DRIZZLE
                    200 => WeatherCondition.THUNDERSTORM, // Thundery outbreaks wordt THUNDERSTORM
                    227 => WeatherCondition.SNOW, // Blowing snow wordt SNOW
                    230 => WeatherCondition.SNOW, // Blizzard wordt SNOW
                    248 => WeatherCondition.FOG,
                    260 => WeatherCondition.FOG, // Freezing fog wordt FOG
                    263 => WeatherCondition.DRIZZLE, // Patchy light drizzle wordt DRIZZLE
                    266 => WeatherCondition.DRIZZLE, // Light drizzle wordt DRIZZLE
                    281 => WeatherCondition.DRIZZLE, // Freezing drizzle wordt DRIZZLE
                    284 => WeatherCondition.DRIZZLE, // Heavy freezing drizzle wordt DRIZZLE
                    293 => WeatherCondition.RAIN, // Patchy light rain wordt RAIN
                    296 => WeatherCondition.RAIN, // Light rain wordt RAIN
                    299 => WeatherCondition.RAIN, // Moderate rain at times wordt RAIN
                    302 => WeatherCondition.RAIN, // Moderate rain wordt RAIN
                    305 => WeatherCondition.RAIN, // Heavy rain at times wordt RAIN
                    308 => WeatherCondition.RAIN, // Heavy rain wordt RAIN
                    311 => WeatherCondition.RAIN, // Light freezing rain wordt RAIN
                    314 => WeatherCondition.RAIN, // Moderate of heavy freezing rain wordt RAIN
                    317 => WeatherCondition.SNOW, // Light sleet wordt SNOW
                    320 => WeatherCondition.SNOW, // Moderate or heavy sleet wordt SNOW
                    323 => WeatherCondition.SNOW, // Patchy light snow wordt SNOW
                    326 => WeatherCondition.SNOW, // Light snow wordt SNOW
                    329 => WeatherCondition.SNOW, // Patchy moderate snow wordt SNOW
                    332 => WeatherCondition.SNOW, // Moderate snow wordt SNOW
                    335 => WeatherCondition.SNOW, // Patchy heavy snow wordt SNOW
                    338 => WeatherCondition.SNOW, // Heavy snow wordt SNOW
                    350 => WeatherCondition.HAIL, // Ice pellets wordt HAIL
                    353 => WeatherCondition.RAIN, // Light rain shower wordt RAIN
                    356 => WeatherCondition.RAIN, // Moderate or heavy rain shower wordt RAIN
                    359 => WeatherCondition.RAIN, // Torrential rain shower wordt RAIN
                    362 => WeatherCondition.SNOW, // Light sleet showers wordt SNOW
                    365 => WeatherCondition.SNOW, // Moderate or heavy sleet showers wordt SNOW
                    368 => WeatherCondition.SNOW, // Light snow showers wordt SNOW
                    371 => WeatherCondition.SNOW, // Moderate or heavy snow showers wordt SNOW
                    374 => WeatherCondition.HAIL, // Light showers of ice pellets wordt HAIL
                    377 => WeatherCondition.HAIL, // Moderate or heavy showers of ice pellets wordt HAIL
                    386 => WeatherCondition.THUNDERSTORM, // Patchy light rain with thunder wordt THUNDERSTORM
                    389 => WeatherCondition.THUNDERSTORM, // Moderate or heavy rain with thunder wordt THUNDERSTORM
                    392 => WeatherCondition.THUNDERSTORM, // Patchy light snow with thunder wordt THUNDERSTORM
                    395 => WeatherCondition.THUNDERSTORM, // Moderate or heavy snow with thunder wordt THUNDERSTORM
                    _ => WeatherCondition.UNKNOWN,
                };
            }
            else
            {
                Debug.WriteLine("Invalid data type or unable to parse to integer.");
                return WeatherCondition.UNKNOWN;
            }
        }


    }
}
