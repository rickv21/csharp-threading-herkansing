﻿using Microsoft.Maui.Controls;
using Newtonsoft.Json.Linq;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    internal class WeerLiveAPI : WeatherService
    {
        public WeerLiveAPI() : base("WeerLive", " https://weerlive.nl/api/weerlive_api_v2.php?key=", 300, -1)
        {
        }

        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, string location, bool simulate = false)
        {
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                    Data = null
                };
            }

            string responseBody;
            if (simulate)
            {
                responseBody = GetTestJSON();
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}{_apiKey}&locatie={location}";
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                        var errorResponse = JObject.Parse(responseBody);
                        string errorCode = errorResponse["cod"]?.ToString() ?? "Unknown Code";
                        string errorMessage = errorResponse["message"]?.ToString() ?? "Unknown Error";
                        return new APIResponse<List<WeatherDataModel>>
                        {
                            Success = false,
                            ErrorMessage = $"{errorCode} - {errorMessage}",
                            Data = null
                        };
                    }
                    CountRequest(); // Important: this counts the requests for the limit.
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

                JObject weatherResponse = JObject.Parse(responseBody);

                // Extract "list" element or throw an exception if not found
                var liveWeer = weatherResponse["liveweer"] ?? throw new Exception("Missing list data in API response");

                var weatherData = new List<WeatherDataModel>();


                // Extract "main" and "weather" data
                JToken? temp = liveWeer[0]["temp"];
                JToken? weather = liveWeer[0]["image"];
                JToken? humidity = liveWeer[0]["lv"];

                // Ensure all necessary data exists
                if (temp == null || weather == null || humidity == null)
                {
                    throw new Exception($"Missing data in API response");
                }
                var condition = CalculateWeatherCondition((string)weather);
                DateTime forecastDate = DateTime.Parse((string)liveWeer[0]["time"]!);

                // Add the weather data to the list
                weatherData.Add(new WeatherDataModel(
                    condition,
                    forecastDate,
                    minTemperature: (double)temp,
                    maxTemperature: (double)temp,
                    humidity: (double)humidity
                ));

                // Return the response
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = true,
                    ErrorMessage = null,
                    Data = weatherData
                };
            }



        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(string location, bool simulate = false)
        {
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                    Data = null
                };
            }

            string responseBody;
            if (simulate)
            {
                responseBody = GetTestJSON();
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}{_apiKey}&locatie={location}";
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                        var errorResponse = JObject.Parse(responseBody);
                        string errorCode = errorResponse["cod"]?.ToString() ?? "Unknown Code";
                        string errorMessage = errorResponse["message"]?.ToString() ?? "Unknown Error";
                        return new APIResponse<List<WeatherDataModel>>
                        {
                            Success = false,
                            ErrorMessage = $"{errorCode} - {errorMessage}",
                            Data = null
                        };
                    }
                    CountRequest(); // Important: this counts the requests for the limit.
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            JObject weatherResponse = JObject.Parse(responseBody);

            // Extract "list" element or throw an exception if not found
            var weekPredictions = weatherResponse["wk_verw"] ?? throw new Exception("Missing list data in API response");
            var weatherData = new List<WeatherDataModel>();

            foreach (var day in weekPredictions)
            {
                var condition = CalculateWeatherCondition((string)day["image"]);
                DateTime forecastDate = DateTime.Parse((string)day["dag"]!);
                var minTemp = day["min_temp"];
                var maxTemp = day["max_temp"];

                weatherData.Add(new WeatherDataModel(
                   condition,
                   forecastDate,
                   minTemperature: (double)minTemp,
                   maxTemperature: (double)maxTemp,
                   humidity: -1.0
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
            switch ((string)data)
            {
                case "zonnig":
                    return WeatherCondition.SUNNY;
                case "bliksem":
                    return WeatherCondition.THUNDERSTORM;
                case "regen":
                    return WeatherCondition.RAIN;
                case "buien":
                    return WeatherCondition.RAIN;
                case "hagel":
                    return WeatherCondition.HAIL;
                case "mist":
                    return WeatherCondition.MIST;
                case "sneeuw":
                    return WeatherCondition.SNOW;
                case "bewolkt":
                    return WeatherCondition.CLOUDY;
                case "lichtbewolkt":
                    return WeatherCondition.PARTLY_CLOUDY;
                case "halfbewolkt":
                    return WeatherCondition.PARTLY_CLOUDY;
                case "halfbewolkt_regen":
                    return WeatherCondition.RAIN;
                case "zwaarbewolkt":
                    return WeatherCondition.CLOUDY;
                case "nachtmist":
                    return WeatherCondition.MIST;
                case "helderenacht":
                    return WeatherCondition.CLEAR;
                case "nachtbewolkt":
                    return WeatherCondition.CLOUDY;
            }
            return WeatherCondition.UNKNOWN;
        }

        protected override string GetTestJSON()
        {
            throw new NotImplementedException();
        }
    }
}
