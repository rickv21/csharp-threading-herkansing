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
            var condition = data.ToString()?.ToLowerInvariant();
            return condition switch
            {
                "sunny" => WeatherCondition.SUNNY,
                "thunderstorm" => WeatherCondition.THUNDERSTORM,
                "rain" => WeatherCondition.RAIN,
                "hail" => WeatherCondition.HAIL,
                "mist" => WeatherCondition.MIST,
                "snow" => WeatherCondition.SNOW,
                "cloudy" => WeatherCondition.CLOUDY,
                "partly cloudy" => WeatherCondition.PARTLY_CLOUDY,
                "clear" => WeatherCondition.CLEAR,
                _ => WeatherCondition.UNKNOWN,
            };
        }
    }
}
