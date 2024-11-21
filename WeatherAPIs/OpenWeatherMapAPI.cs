using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    public class OpenWeatherMapAPI : WeatherService
    {
        public OpenWeatherMapAPI() : base("Test", "https://api.openweathermap.org/data/2.5/", 1000, -1)
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
                CountRequest(); // Important: this counts the requests for the limit.
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}forecast?q={location}&appid={_apiKey}&units=metric";
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

            Debug.Write(responseBody);
            JObject weatherResponse = JObject.Parse(responseBody);
            var list = weatherResponse["list"] ?? throw new Exception("Missing list data in API response");
            var weatherData = new List<WeatherDataModel>();

            foreach (var item in list)
            {
                JToken? main = item["main"];
                JToken? weather = item["weather"]?.FirstOrDefault();
                if (item["dt_txt"] == null || main == null || main["temp_min"] == null || main["temp_max"] == null || main["humidity"] == null || weather == null || weather["id"] == null)
                { 
                    throw new Exception($"Missing data in API response at {item}"); 
                }
                DateTime forecastDate = DateTime.Parse((string)item["dt_txt"]!);
                if(forecastDate.Date == day.Date)
                {
                    int weatherId = (int)weather["id"]!;
                    WeatherCondition condition = CalculateWeatherCondition(weatherId);

                    weatherData.Add(new WeatherDataModel(
                        condition,
                        forecastDate,
                        minTemperature: (double)main["temp_min"]!,
                        maxTemperature: (double)main["temp_max"]!,
                        humidity: (double)main["humidity"]!
                        ));
                }
            }

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
                CountRequest(); // Important: this counts the requests for the limit.
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}forecast?q={location}&appid={_apiKey}&units=metric";
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

            Debug.Write(responseBody);
            JObject weatherResponse = JObject.Parse(responseBody);

            // Check if weatherResponse["list"] exists and is not null
            if (weatherResponse["list"] == null)
            {
                throw new Exception("Missing list data in API response");
            }

            var weatherData = new List<WeatherDataModel>();
            var dailyData = new Dictionary<DateTime, List<JToken>>();
            var list = weatherResponse["list"] ?? throw new Exception("Missing list data in API response");

            foreach (var item in list)
            {
                JToken? main = item["main"];
                JToken? weather = item["weather"]?.FirstOrDefault();

                if (item["dt_txt"] == null || main == null || main["temp_min"] == null || main["temp_max"] == null || main["humidity"] == null || weather == null || weather["id"] == null)
                {
                    throw new Exception($"Missing data in API response at {item}");
                }

                DateTime forecastDate = DateTime.Parse((string)item["dt_txt"]!);
                if (!dailyData.ContainsKey(forecastDate.Date))
                {
                    dailyData[forecastDate.Date] = new List<JToken>();
                }
                dailyData[forecastDate.Date].Add(item);
            }

            foreach (var day in dailyData.Keys)
            {
                var dayData = dailyData[day];
                // Calculate the weather condition for the day based on the first weather_id
                var firstWeatherId = (int)dayData.First()["weather"]!.First()["id"]!; 
                WeatherCondition condition = CalculateWeatherCondition(firstWeatherId);

                weatherData.Add(new WeatherDataModel(
                    condition,
                    day,
                    minTemperature: dayData.Min(d => (double)d["main"]!["temp_min"]!),
                    maxTemperature: dayData.Max(d => (double)d["main"]!["temp_max"]!),
                    humidity: dayData.Average(d => (double)d["main"]!["humidity"]!)
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
            int id = (int)data;
            char firstDigitChar = id.ToString()[0]; 
            int firstDigit = int.Parse(firstDigitChar.ToString());

            switch (firstDigit)
            {
                case 2:
                    return WeatherCondition.THUNDERSTORM;
                case 3:
                    return WeatherCondition.DRIZZLE;
                case 5: 
                    return WeatherCondition.RAIN;
                case 6:
                    return WeatherCondition.SNOW;
                case 7:
                    {
                        if(id == 701)
                        {
                            return WeatherCondition.MIST;
                        }
                        if (id == 711)
                        {
                            return WeatherCondition.SMOKE;
                        }
                        if (id == 721)
                        {
                            return WeatherCondition.HAZE;
                        }
                        if(id == 731)
                        {
                            return WeatherCondition.DUST;
                        }
                        if (id == 741)
                        {
                            return WeatherCondition.FOG;
                        }
                        if (id == 751)
                        {
                            return WeatherCondition.SAND;
                        }
                        if (id == 761)
                        {
                            return WeatherCondition.DUST;
                        }
                        if (id == 762)
                        {
                            return WeatherCondition.ASH;
                        }
                        if (id == 771)
                        {
                            return WeatherCondition.SQUALL;
                        }
                        if (id == 781)
                        {
                            return WeatherCondition.TORNADO;
                        }
                        Debug.WriteLine("Unknown weather code: " +  id);
                        return WeatherCondition.UNKNOWN;
                    }
                case 8:
                    {
                        if (id == 800)
                        {
                            return WeatherCondition.SUNNY;
                        } 
                        else
                        {
                            return WeatherCondition.CLOUDY;
                        }
                        
                    }
                default:
                    Debug.WriteLine("Unknown weather code: " + id);
                    return WeatherCondition.UNKNOWN;
            }

        }

        protected override string GetTestJSON()
        {
            //TODO: This is very long, might put this in a external file later.

            //The temperature is given in Kelvin in this example code.
            //This will be replaced with actual query data once the API key is working correctly.
            string json =  @"
    {
      ""cod"": ""200"",
      ""message"": 0,
      ""cnt"": 40,
      ""list"": [
        {
          ""dt"": 1661871600,
          ""main"": {
            ""temp"": 296.76,
            ""feels_like"": 296.98,
            ""temp_min"": 296.76,
            ""temp_max"": 297.87,
            ""pressure"": 1015,
            ""sea_level"": 1015,
            ""grnd_level"": 933,
            ""humidity"": 69,
            ""temp_kf"": -1.11
          },
          ""weather"": [
            {
              ""id"": 500,
              ""main"": ""Rain"",
              ""description"": ""light rain"",
              ""icon"": ""10d""
            }
          ],
          ""clouds"": {
            ""all"": 100
          },
          ""wind"": {
            ""speed"": 0.62,
            ""deg"": 349,
            ""gust"": 1.18
          },
          ""visibility"": 10000,
          ""pop"": 0.32,
          ""rain"": {
            ""3h"": 0.26
          },
          ""sys"": {
            ""pod"": ""d""
          },
          ""dt_txt"": ""2022-08-30 15:00:00""
        },
        {
          ""dt"": 1661882400,
          ""main"": {
            ""temp"": 295.45,
            ""feels_like"": 295.59,
            ""temp_min"": 292.84,
            ""temp_max"": 295.45,
            ""pressure"": 1015,
            ""sea_level"": 1015,
            ""grnd_level"": 931,
            ""humidity"": 71,
            ""temp_kf"": 2.61
          },
          ""weather"": [
            {
              ""id"": 500,
              ""main"": ""Rain"",
              ""description"": ""light rain"",
              ""icon"": ""10n""
            }
          ],
          ""clouds"": {
            ""all"": 96
          },
          ""wind"": {
            ""speed"": 1.97,
            ""deg"": 157,
            ""gust"": 3.39
          },
          ""visibility"": 10000,
          ""pop"": 0.33,
          ""rain"": {
            ""3h"": 0.57
          },
          ""sys"": {
            ""pod"": ""n""
          },
          ""dt_txt"": ""2022-08-30 18:00:00""
        },
        {
          ""dt"": 1661893200,
          ""main"": {
            ""temp"": 292.46,
            ""feels_like"": 292.54,
            ""temp_min"": 290.31,
            ""temp_max"": 292.46,
            ""pressure"": 1015,
            ""sea_level"": 1015,
            ""grnd_level"": 931,
            ""humidity"": 80,
            ""temp_kf"": 2.15
          },
          ""weather"": [
            {
              ""id"": 500,
              ""main"": ""Rain"",
              ""description"": ""light rain"",
              ""icon"": ""10n""
            }
          ],
          ""clouds"": {
            ""all"": 68
          },
          ""wind"": {
            ""speed"": 2.66,
            ""deg"": 210,
            ""gust"": 3.58
          },
          ""visibility"": 10000,
          ""pop"": 0.7,
          ""rain"": {
            ""3h"": 0.49
          },
          ""sys"": {
            ""pod"": ""n""
          },
          ""dt_txt"": ""2022-08-30 21:00:00""
        },
        {
          ""dt"": 1662292800,
          ""main"": {
            ""temp"": 294.93,
            ""feels_like"": 294.83,
            ""temp_min"": 294.93,
            ""temp_max"": 294.93,
            ""pressure"": 1018,
            ""sea_level"": 1018,
            ""grnd_level"": 935,
            ""humidity"": 64,
            ""temp_kf"": 0
          },
          ""weather"": [
            {
              ""id"": 804,
              ""main"": ""Clouds"",
              ""description"": ""overcast clouds"",
              ""icon"": ""04d""
            }
          ],
          ""clouds"": {
            ""all"": 88
          },
          ""wind"": {
            ""speed"": 1.14,
            ""deg"": 17,
            ""gust"": 1.57
          },
          ""visibility"": 10000,
          ""pop"": 0,
          ""sys"": {
            ""pod"": ""d""
          },
          ""dt_txt"": ""2022-09-04 12:00:00""
        }
      ],
      ""city"": {
        ""id"": 3163858,
        ""name"": ""Zocca"",
        ""coord"": {
          ""lat"": 44.34,
          ""lon"": 10.99
        },
        ""country"": ""IT"",
        ""population"": 4593,
        ""timezone"": 7200,
        ""sunrise"": 1661834187,
        ""sunset"": 1661882248
      }
    }";
            DateTime date = DateTime.Now;
            json = json.Replace("2022-08-30", date.ToString("yyyy-MM-dd"));

            date = DateTime.Now.AddDays(2);
            json = json.Replace("2022-09-04", date.ToString("yyyy-MM-dd"));
            return json;
        }
    }
}
