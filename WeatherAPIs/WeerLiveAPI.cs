using Microsoft.Maui.Controls;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    internal class WeerLiveAPI : WeatherService
    {
        public WeerLiveAPI() : base("WeerLive", " https://weerlive.nl/api/weerlive_api_v2.php?key=", 300, -1)
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
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                    Data = null
                };
            }

            string responseBody;
            if (simulate)
            {
                responseBody = GetTestJSON("weer_live_test.json");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}{_apiKey}&locatie={location.Name}";
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
            Debug.WriteLine(responseBody);
            JObject weatherResponse = JObject.Parse(responseBody);

            // Extract "list" element or throw an exception if not found
            var hourPredictions = weatherResponse["uur_verw"] ?? throw new Exception("Missing list data in API response");

            var weatherData = new List<WeatherDataModel>();
        

            bool setTestDay = false;
            foreach (var hour in hourPredictions)
            {
                var condition = CalculateWeatherCondition((string)hour["image"]);
                DateTime forecastDate = DateTime.Parse((string)hour["uur"]!);

                if (simulate && !setTestDay)
                {
                    day = forecastDate;
                    setTestDay = true;
                }
                if (forecastDate.Date != day.Date)
                {
                    Debug.WriteLine($"Skipping entry for {Name} as date ({forecastDate}) does not match.");
                    continue; // Skip entries not matching the requested day (only when not simulating).
                }

                var minTemp = hour["temp"];
                var maxTemp = hour["temp"];

                weatherData.Add(new WeatherDataModel(
                    Name,
                   condition,
                   forecastDate,
                   minTemperature: (double)minTemp,
                   maxTemperature: (double)maxTemp,
                   humidity: -1.0
               ));
            
            }

            // Return the response
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
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                    Data = null
                };
            }

            string responseBody;
            if (simulate)
            {
                responseBody = GetTestJSON("weer_live_test.json");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}{_apiKey}&locatie={location.Name}";
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
            Debug.WriteLine(responseBody);
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
                   Name,
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
                case "wolkennacht":
                    return WeatherCondition.CLOUDY;
            }
            return WeatherCondition.UNKNOWN;
        }
    }
}
