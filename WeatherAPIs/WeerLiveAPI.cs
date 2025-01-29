using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    public class WeerLiveAPI : WeatherService
    {
        public WeerLiveAPI() : base("WeerLive", " https://weerlive.nl/api/weerlive_api_v2.php?key=", 300, -1)
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
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                    Source = Name
                };
            }

            string responseBody;

            using (HttpClient client = new())
            {
                string url = $"{_baseURL}{_apiKey}&locatie={location.Name}";
                HttpResponseMessage response = await client.GetAsync(url);

                responseBody = await response.Content.ReadAsStringAsync();

                try
                {
                    var jsonResponse = JObject.Parse(responseBody);
                    var liveweerArray = jsonResponse["liveweer"] as JArray;

                    if (liveweerArray != null && liveweerArray.Count > 0)
                    {
                        var firstElement = liveweerArray.First();
                        if (firstElement["fout"] != null)
                        {
                            string errorMessage = firstElement["fout"].ToString();
                            return new APIResponse<List<WeatherDataModel>>
                            {
                                Success = false,
                                ErrorMessage = $"API Error: {errorMessage}",
                                Source = Name
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JSON Parsing Error: {ex.Message}");
                    return new APIResponse<List<WeatherDataModel>>
                    {
                        Success = false,
                        ErrorMessage = "Invalid JSON response from API.",
                        Source = Name
                    };
                }
            }

            JObject weatherResponse = JObject.Parse(responseBody);

            // Extract "list" element or throw an exception if not found
            var hourPredictions = weatherResponse["uur_verw"] ?? throw new Exception("Missing list data in API response");

            var weatherData = new List<WeatherDataModel>();
        
            foreach (var hour in hourPredictions)
            {
                var condition = CalculateWeatherCondition((string)hour["image"]);
                DateTime forecastDate = DateTime.Parse((string)hour["uur"]!);

                if (forecastDate.Date != day.Date)
                {
                    continue; // Skip entries not matching the requested day (only when not simulating).
                }

                var minTemp = hour["temp"];
                var maxTemp = hour["temp"];

                weatherData.Add(new WeatherDataModel(
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
                    Data = weatherData,
                    Source = Name
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
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                    Source = Name
                };
            }

            string responseBody;

            using (HttpClient client = new())
            {
                string url = $"{_baseURL}{_apiKey}&locatie={location.Name}";
                HttpResponseMessage response = await client.GetAsync(url);

                responseBody = await response.Content.ReadAsStringAsync();

                // Handle API error messages inside 200 responses
                try
                {
                    var jsonResponse = JObject.Parse(responseBody);
                    var liveweerArray = jsonResponse["liveweer"] as JArray;

                    if (liveweerArray != null && liveweerArray.Count > 0)
                    {
                        var firstElement = liveweerArray.First();
                        if (firstElement["fout"] != null)
                        {
                            string errorMessage = firstElement["fout"].ToString();
                            return new APIResponse<List<WeatherDataModel>>
                            {
                                Success = false,
                                ErrorMessage = $"API Error: {errorMessage}",
                                Source = Name
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JSON Parsing Error: {ex.Message}");
                    return new APIResponse<List<WeatherDataModel>>
                    {
                        Success = false,
                        ErrorMessage = "Invalid JSON response from API.",
                        Source = Name
                    };
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
                default:
                    return WeatherCondition.UNKNOWN;
            }
        }
    }
}
