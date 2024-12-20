using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;
using WeatherApp.Models;
using WeatherApp.Utils;

namespace WeatherApp.WeatherAPIs
{
    public class VisualCrossingAPI : WeatherService
    {
        public VisualCrossingAPI() : base("Visual Crossing", "https://weather.visualcrossing.com/VisualCrossingWebServices", 50, -1)
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
                responseBody = GetTestJSON("visual_crossing_test.json");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    var latitude = location.Latitude.ToString().Replace(",", ".");
                    var longitude = location.Longitude.ToString().Replace(",", ".");
                    string url = $"{_baseURL}/rest/services/timeline/{latitude}%2C%20{longitude}?unitGroup=metric&key={_apiKey}&contentType=json&lang=id";
                    Debug.WriteLine(url);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                        int errorCode = (int)response.StatusCode;
                        var errorData = responseBody;
                        string errorMessage;
                        if (errorData == null)
                        {
                            errorMessage = "Could not get error information.";
                        }
                        else
                        {
                            errorMessage = errorData ?? "Unknown Error";
                        }
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
            JObject weatherObject = JObject.Parse(responseBody);
            JArray daysArray = (JArray)weatherObject["days"]!;
            var weatherData = new List<WeatherDataModel>();
            bool setTestDay = false; 
            foreach (var dayItem in daysArray)
            {
                try
                {
                    string dateString = (string)dayItem["datetime"]!;
                    DateTime forecastDate = DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

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

                    JArray hoursArray = (JArray)dayItem["hours"]!;
                    foreach (var hourItem in hoursArray)
                    {
                        try
                        {
                            string hourDateString = (string)hourItem["datetime"]!;
                            DateTime hourDateTime = DateTime.ParseExact(hourDateString, "HH:mm:ss", CultureInfo.InvariantCulture);

                            // Combine the date and time for a full timestamp
                            DateTime timestamp = forecastDate.Date.Add(hourDateTime.TimeOfDay);

                            double temperature = (double)hourItem["temp"]!;
                            double humidity = (double?)hourItem["humidity"] ?? -1;
                            string conditions = (string)hourItem["conditions"]!;

                            //Since it is called conditions, maybe it can have more than 1??
                            //I do not see it giving more than 1 at the moment, so let's make sure that it is indeed always only giving 1.
                            Debug.Assert(!conditions.Contains(","), $"More than 1 condition was given (day-{hourDateString}, comma seperator), this should not happen.\nPost this in the group chat!!");
                            Debug.Assert(!conditions.Contains(" "), $"More than 1 condition was given (dat-{hourDateString}, space seperator), this should not happen.\nPost this in the group chat!!");

                            WeatherCondition condition = CalculateWeatherCondition(conditions);

                            weatherData.Add(new WeatherDataModel(
                                condition: condition,
                                timeStamp: timestamp,
                                minTemperature: temperature,
                                maxTemperature: temperature, // Assume no separate max/min per hour
                                humidity: humidity
                            ));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing hourly weather item: {ex.Message}");
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing weather item: {ex.Message}");
                    continue;
                }
            }

            if (weatherData.Count == 0)
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    ErrorMessage = "The API did not return data for the given datatime.",
                };
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
                    ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                    Data = null
                };
            }

            string responseBody;
            if (simulate)
            {
                responseBody = GetTestJSON("visual_crossing_test.json");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    var latitude = location.Latitude.ToString().Replace(",", ".");
                    var longitude = location.Longitude.ToString().Replace(",", ".");
                    string url = $"{_baseURL}/rest/services/timeline/{latitude}%2C%20{longitude}?unitGroup=metric&key={_apiKey}&contentType=json&lang=id";
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                        int errorCode = (int)response.StatusCode;
                        var errorData = responseBody;
                        string errorMessage;
                        if (errorData == null)
                        {
                            errorMessage = "Could not get error information.";
                        }
                        else
                        {
                            errorMessage = errorData ?? "Unknown Error";
                        }

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

            var daysArray = weatherResponse["days"] ?? throw new Exception("Missing days data in API response");
            var weatherData = new List<WeatherDataModel>();

            foreach (var dayItem in daysArray)
            {
                try
                {
                    string dateString = (string)dayItem["datetime"]!;
                    Debug.WriteLine(dateString);
                    DateTime forecastDate = DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    double minTemperature = (double)dayItem["tempmin"]!;
                    double maxTemperature = (double)dayItem["tempmax"]!;
                    double averageHumidity = (double?)dayItem["humidity"] ?? -1;
                    string conditionText = (string)dayItem["conditions"] ?? "";

                    //Since it is called conditions, maybe it can have more than 1??
                    //I do not see it giving more than 1 at the moment, so let's make sure that it is indeed always only giving 1.
                    Debug.Assert(!conditionText.Contains(","), $"More than 1 condition was given (week-{dateString}, comma seperator), this should not happen.\nPost this in the group chat!!");
                    Debug.Assert(!conditionText.Contains(" "), $"More than 1 condition was given (week-{dateString}, space seperator), this should not happen.\nPost this in the group chat!!");

                    WeatherCondition condition = CalculateWeatherCondition(conditionText);

                    weatherData.Add(new WeatherDataModel(
                        condition: condition,
                        timeStamp: forecastDate,
                        minTemperature: minTemperature,
                        maxTemperature: maxTemperature, //It only gives 1 temperature value back.
                        humidity: averageHumidity
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
                Data = weatherData
            };
        }

        protected override WeatherCondition CalculateWeatherCondition(object data)
        {
            string conditionText = data.ToString()?.ToLowerInvariant() ?? "";

            return conditionText switch
            {
                "clear" or "clearingpm" or "sunshine" or "norain" => WeatherCondition.CLEAR,
                "cloudierpm" or "cloudcover" or "overcast" or "type_27" or "type_28" or "type_29" or "type_41" => WeatherCondition.CLOUDY,
                "coolingdown" or "cold" => WeatherCondition.COLD,
                "dew" or "heatindex" or "warmingup" or "type_20" or "type_39" => WeatherCondition.UNKNOWN, // Includes less specific or undefined conditions
                "rainallday" or "rainam" or "rainampm" or "rainchance" or "raindefinite" or "rainearlyam" or "rainlatepm" or "rainpm" or "type_21" or "type_26" or "type_24" or "type_25" => WeatherCondition.RAIN, // Consolidated heavy rain and rain types
                "rainsnowallday" or "rainsnowam" or "rainsnowampm" or "rainsnowchance" or "rainsnowdefinite" or "rainsnowearlyam" or "rainsnowlatepm" or "rainsnowpm" or "type_22" or "type_23" or "type_32" or "type_33" => WeatherCondition.RAIN, // Mapped rain and snow together to RAIN
                "snowallday" or "snowam" or "snowampm" or "snowchance" or "snowclearinglater" or "snowdefinite" or "snowearlyam" or "snowlatepm" or "snowpm" or "type_1" or "type_31" or "type_34" or "type_35" => WeatherCondition.SNOW,
                "stormspossible" or "stormsstrong" or "type_37" or "type_38" or "type_18" => WeatherCondition.THUNDERSTORM, // Includes lightning as part of thunderstorms
                "variablecloud" or "type_42" => WeatherCondition.PARTLY_CLOUDY,
                "fog" or "type_8" or "type_12" => WeatherCondition.FOG, // Includes freezing fog
                "mist" or "type_19" => WeatherCondition.MIST,
                "dust" or "dust storm" or "type_7" => WeatherCondition.DUST,
                "haze" or "smoke" or "type_30" => WeatherCondition.HAZE,
                "tornado" or "type_15" => WeatherCondition.TORNADO,
                "squalls" or "type_36" => WeatherCondition.SQUALL,
                "windy" => WeatherCondition.WINDY,
                "type_2" or "type_4" or "type_6" => WeatherCondition.DRIZZLE, // Includes light drizzle types
                "type_3" or "type_5" => WeatherCondition.RAIN, // Heavy drizzle consolidated into rain
                "type_9" or "type_10" or "type_13" or "type_11" or "type_14" => WeatherCondition.RAIN, // Freezing rain types consolidated into rain
                "type_16" or "type_40" => WeatherCondition.HAIL,
                "type_17" => WeatherCondition.ICE,
                "type_43" => WeatherCondition.CLEAR,
                _ => WeatherCondition.UNKNOWN // Default fallback
            };
        }


    }
}
