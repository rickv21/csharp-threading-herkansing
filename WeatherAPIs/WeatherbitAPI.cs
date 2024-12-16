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

        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location, bool simulate = false)
        {
           throw new NotImplementedException();
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
                responseBody = GetTestJSON("weatherbit_api_test.json");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}?key={_apiKey}&city={location.Name}&days=7";  // 7 days
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

                double humidity = day["humidity"] != null ? (double)day["humidity"] : 0;

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

        protected override WeatherCondition CalculateWeatherCondition(object data)
        {
        } 
    }
}
