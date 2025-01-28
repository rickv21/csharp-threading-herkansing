using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;
using WeatherApp.Models;
using WeatherApp.Utils;

namespace WeatherApp.WeatherAPIs
{
    public class AccuWeatherAPI : WeatherService
    {
        public AccuWeatherAPI() : base("AccuWeather", "http://dataservice.accuweather.com", 50, -1)
        {
        }

        /// <summary>
        /// Gets the location key for the given location via the AccuWeather geolocation API.
        /// </summary>
        /// <param name="location">The location data.</param>
        /// <param name="simulate">If the request should use test data.</param>
        /// <returns>
        /// An API Response containing the location key if successful
        /// Otherwise it contains an errorMessage.
        /// </returns>
        private async Task<APIResponse<string>> GetLocationKey(LocationModel location, bool simulate)
        {
            Debug.WriteLine($"Requesting location key for {Name}.");
            if (HasReachedRequestLimit())
            {
                return new APIResponse<string>
                {
                    Success = false,
                    Source = Name,
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                };
            }

            string responseBody;

            if (simulate)
            {
                responseBody = GetTestJSON("accu_weather_location_test.json");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}/locations/v1/cities/geoposition/search?apikey={_apiKey}&q={location.Latitude},{location.Longitude}&details=true";
                    Debug.WriteLine(url);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                        var errorResponse = JObject.Parse(responseBody);
                        int errorCode = (int)response.StatusCode;

                        var errorData = errorResponse["fault"];
                        string errorMessage;
                        if (errorData == null)
                        {
                            errorMessage = "Could not get error information.";
                        }
                        else
                        {
                            errorMessage = errorData["faultstring"]?.ToString() ?? "Unknown Error";
                        }

                        return new APIResponse<string>
                        {
                            Success = false,
                            Source = Name,
                            ErrorMessage = $"{errorCode} - {errorMessage}",
                            Data = null
                        };
                    }
                    CountRequest(); // Important: this counts the requests for the limit.
                    responseBody = await response.Content.ReadAsStringAsync();
        
                }
            }
            JObject locationResponse = JObject.Parse(responseBody);
            var locationKey = locationResponse["Key"];
            if (locationKey == null)
            {
                return new APIResponse<string>
                {
                    Success = false,
                    Source = Name,
                    ErrorMessage = "Could not get locationKey."
                };
            }

            return new APIResponse<string>
            {
                Success = true,
                Source = Name,
                Data = locationKey.ToString()
            };
        
        }

        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location, bool simulate = false)
        {
            Debug.WriteLine($"Requesting day data for {Name}.");
            JsonFileManager jsonFileManager = new();

            //Check if locationKey existst in the JSON file, otherwise get it from AccuWeather.
            string? locationKey = jsonFileManager.GetData("data", Name, "storedPlaceKeys", location.Name) as string;
            Debug.WriteLine(locationKey);
            if (locationKey == null)
            {
                Debug.WriteLine($"Obtaining location key for {location.Name} from AccuWeather API.");
                APIResponse<string> locationKeyResponse = await GetLocationKey(location, simulate);
                if (!locationKeyResponse.Success)
                    return new APIResponse<List<WeatherDataModel>>
                    {
                        Success = false,
                        Source = Name,
                        ErrorMessage = locationKeyResponse.ErrorMessage,
                    };
                {
                }
                Debug.Assert(locationKeyResponse.Data != null);
                locationKey = locationKeyResponse.Data;
                jsonFileManager.SetData(locationKey, "data", Name, "storedPlaceKeys", location.Name);
            }

            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    Source = Name,
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                };
            }

            string responseBody;
            if (simulate)
            {
                responseBody = GetTestJSON("accu_weather_hour_test.json");
                CountRequest(); // Important: this counts the requests for the limit.
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}/forecasts/v1/hourly/12hour/{locationKey}/?apikey={_apiKey}&details=true&metric=true";
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                        var errorResponse = JObject.Parse(responseBody);
                        int errorCode = (int)response.StatusCode;
                        var errorData = errorResponse["fault"];
                        string errorMessage;
                        if (errorData == null)
                        {
                            errorMessage = "Could not get error information.";
                        }
                        else
                        {
                            errorMessage = errorData["faultstring"]?.ToString() ?? "Unknown Error";
                        }
                        return new APIResponse<List<WeatherDataModel>>
                        {
                            Success = false,
                            Source = Name,
                            ErrorMessage = $"{errorCode} - {errorMessage}",
                        };
                    }
                    CountRequest(); // Important: this counts the requests for the limit.
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            Debug.WriteLine(responseBody);
            JArray weatherArray = JArray.Parse(responseBody);
            var weatherData = new List<WeatherDataModel>();
            bool setTestDay = false; 
            foreach (var item in weatherArray)
            {
                try
                {
                    string dateString = (string)item["DateTime"]!;
                    DateTime forecastDate = DateTime.ParseExact(dateString, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    //Some test code so that it still is limited to one day when simulating.
                    if (simulate && !setTestDay)
                    {
                        day = forecastDate;
                        setTestDay = true;
                    }

                    if (forecastDate.Date != day.Date)
                    {
                        Debug.WriteLine($"Skipping entry for {Name} as date ({forecastDate.Date}) does not match.");
                        continue; // Skip entries not matching the requested day (only when not simulating).
                    }

                    double temperature = (double)item["Temperature"]!["Value"]!;
                    int weatherIcon = (int)item["WeatherIcon"]!;
                    string iconPhrase = (string)item["IconPhrase"]!;
                    bool hasPrecipitation = (bool)item["HasPrecipitation"]!;
                    string precipitationType = item["PrecipitationType"]?.ToString() ?? "None";
                    string precipitationIntensity = item["PrecipitationIntensity"]?.ToString() ?? "None";
                    int precipitationProbability = (int)item["PrecipitationProbability"]!;

                    WeatherCondition condition = CalculateWeatherCondition(weatherIcon);

                    weatherData.Add(new WeatherDataModel(
                        condition: condition,
                        timeStamp: forecastDate,
                        minTemperature: temperature,
                        maxTemperature: temperature, //It only gives 1 temperature value back.
                        humidity: -1 //It does not give humidity back.
                    ));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing weather item: {ex.Message}");
                    continue;
                }
            }

            if (weatherArray.Count == 0)
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    Source = Name,
                    ErrorMessage = "The API did not return data for the given datatime.",
                };
            }


            return new APIResponse<List<WeatherDataModel>>
            {
                Success = true,
                Source = Name,
                Data = weatherData
            };
        }

        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(LocationModel location, bool simulate = false)
        {
            Debug.WriteLine($"Requesting week data for {Name}.");
            JsonFileManager jsonFileManager = new JsonFileManager();

            string? locationKey = jsonFileManager.GetData("data", Name, "storedPlaceKeys", location.Name) as string;
            Debug.WriteLine(locationKey);
            if (locationKey == null)
            {
                Debug.WriteLine($"Obtaining location key for {location.Name} from AccuWeather API.");
                APIResponse<String> locationKeyResponse = await GetLocationKey(location, simulate);
                if (!locationKeyResponse.Success)
                    return new APIResponse<List<WeatherDataModel>>
                    {
                        Success = false,
                        Source = Name,
                        ErrorMessage = locationKeyResponse.ErrorMessage,
                    };
                {
                }
                Debug.Assert(locationKeyResponse.Data != null);
                locationKey = locationKeyResponse.Data;
                string locationName = location.Name!;
                jsonFileManager.SetData(locationKey, "data", Name, "storedPlaceKeys", location.Name);
            }

            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    Source = Name,
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                };
            }

            string responseBody;
            if (simulate)
            {
                responseBody = GetTestJSON("accu_weather_week_test.json");
                CountRequest(); // Important: this counts the requests for the limit.
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}/forecasts/v1/daily/5day/{locationKey}/?apikey={_apiKey}&details=true&metric=true";
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                        var errorResponse = JObject.Parse(responseBody);
                        int errorCode = (int)response.StatusCode;
                        var errorData = errorResponse["fault"];
                        string errorMessage;
                        if (errorData == null)
                        {
                            errorMessage = "Could not get error information.";
                        }
                        else
                        {
                            errorMessage = errorData["faultstring"]?.ToString() ?? "Unknown Error";
                        }

                        return new APIResponse<List<WeatherDataModel>>
                        {
                            Success = false,
                            ErrorMessage = $"{errorCode} - {errorMessage}",
                            Source = Name,
                        };
                    }
                    CountRequest(); // Important: this counts the requests for the limit.
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            Debug.WriteLine(responseBody);
            JObject weatherResponse = JObject.Parse(responseBody);

            var dailyForecasts = weatherResponse["DailyForecasts"] ?? throw new Exception("Missing DailyForecasts data in API response");
            var weatherData = new List<WeatherDataModel>();

            foreach (var forecast in dailyForecasts)
            {
                try
                {
                    string dateString = (string)forecast["Date"]!;
                    Debug.WriteLine(dateString);

                    DateTime forecastDate = DateTime.ParseExact(dateString, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    double minTemperature = (double)forecast["Temperature"]!["Minimum"]!["Value"]!;
                    double maxTemperature = (double)forecast["Temperature"]!["Maximum"]!["Value"]!;
                    double averageHumidity = -1; // Assuming humidity data is not provided in this structure.
                    WeatherCondition dayCondition = CalculateWeatherCondition((int)forecast["Day"]!["Icon"]!);
                    //WeatherCondition nightCondition = CalculateWeatherCondition((int)forecast["Night"]["Icon"]!);

                    weatherData.Add(new WeatherDataModel(
                        condition: dayCondition,
                        timeStamp: forecastDate,
                        minTemperature: minTemperature,
                        maxTemperature: maxTemperature, //It only gives 1 temperature value back.
                        humidity: averageHumidity //It does not give humidity back.
                    ));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing weather forecase: {ex.Message}");
                    continue;
                }
            }

            return new APIResponse<List<WeatherDataModel>>
            {
                Success = true,
                Data = weatherData,
                Source = Name,
            };
        }

        protected override WeatherCondition CalculateWeatherCondition(object data)
        {
            int id = (int)data;

            // Match icon IDs to WeatherCondition
            switch (id)
            {
                case 1:
                    return WeatherCondition.SUNNY;
                case 2:
                case 3:
                case 4:
                case 6:
                    return WeatherCondition.PARTLY_CLOUDY;
                case 5:
                    return WeatherCondition.HAZE;
                case 7:
                case 8:
                case 38:
                    return WeatherCondition.CLOUDY;
                case 11:
                    return WeatherCondition.FOG;
                case 12:
                case 29:
                    return WeatherCondition.RAIN;
                case 13:
                case 14:
                    return WeatherCondition.PARTLY_CLOUDY; // With rain/showers, but primary is cloudy
                case 15:
                case 16:
                case 17:
                    return WeatherCondition.THUNDERSTORM;
                case 18:
                    return WeatherCondition.RAIN;
                case 19:
                case 20:
                case 21:
                    return WeatherCondition.SNOW; // With flurries but primary is snow
                case 22:
                case 23:
                case 44:
                    return WeatherCondition.SNOW;
                case 24:
                case 25:
                case 26:
                    return WeatherCondition.HAIL; // Assumed for ice, sleet, freezing rain
                case 30:
                    return WeatherCondition.SUNNY; // Hot could mean sunny or extreme heat
                case 31:
                    return WeatherCondition.COLD; // Specific cold weather
                case 32:
                    return WeatherCondition.WINDY;
                case 33:
                case 34:
                case 35:
                case 36:
                    return WeatherCondition.PARTLY_CLOUDY;
                case 37:
                    return WeatherCondition.HAZE; // Hazy moonlight at night
                case 39:
                case 40:
                    return WeatherCondition.RAIN; // Rain-related during night
                case 41:
                case 42:
                    return WeatherCondition.THUNDERSTORM; // Night thunderstorms
                case 43:
                    return WeatherCondition.SNOW; // Night flurries
                default:
                    Debug.WriteLine("Unknown weather icon ID: " + id);
                    return WeatherCondition.UNKNOWN;
            }
        }
    }
}
