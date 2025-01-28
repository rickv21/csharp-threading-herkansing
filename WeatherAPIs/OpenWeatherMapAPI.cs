using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    public class OpenWeatherMapAPI : WeatherService
    {

        public OpenWeatherMapAPI() : base("Open Weather Map", "https://api.openweathermap.org/data/2.5/", 1000, -1)
        {
        }

        public string GetMapCode()
        {
            // Kaart
            string htmlContent = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.3/dist/leaflet.css"" />
                    <script src=""https://unpkg.com/leaflet@1.9.3/dist/leaflet.js""></script>
                </head>
                <body>
                    <div id=""map"" style=""width: 100%; height: 100vh;""></div>
                    <script>
                       var map = L.map('map', {
                            center: [52.1326, 5.2913], // Center of the Netherlands
                            zoom: 7, // Initial zoom level
                            minZoom: 7, // Minimum zoom level (zoom out (restricted))
                            maxZoom: 19, // Maximum zoom level (zoom in)
                            maxBounds: [[50.7504, 3.3584], [53.6316, 7.2275]], // Southwest and northeast bounds of NL
                            maxBoundsViscosity: 0.5,
                            wheelPxPerZoomLevel: 60,
                        });

                        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                            attribution: 'Map data � <a href=""https://www.openstreetmap.org/"">OpenStreetMap</a> contributors',
                            maxZoom: 19,
                        }).addTo(map);

                        L.tileLayer('https://tile.openweathermap.org/map/clouds/{z}/{x}/{y}.png?appid=APIKEY', {
                            attribution: 'Cloud data � <a href=""https://openweathermap.org/"">OpenWeatherMap</a>',
                            maxZoom: 19,
                        }).addTo(map);
                    </script>
                </body>
                </html>";

            htmlContent = htmlContent.Replace("APIKEY", _apiKey);

            return htmlContent;
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
                    Source = Name
                };
            }
            string responseBody;
            if (simulate)
            {
                responseBody = GetTestJSON("openweather_test.json");
                CountRequest(); // Important: this counts the requests for the limit.
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}forecast?lat={location.Latitude}&lon={location.Longitude}&appid={_apiKey}&units=metric";
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
                            Source = Name
                        };
                    }
                    CountRequest(); // Important: this counts the requests for the limit.
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            JObject weatherResponse = JObject.Parse(responseBody);
            var list = weatherResponse["list"] ?? throw new Exception("Missing list data in API response");
            var weatherData = new List<WeatherDataModel>();
            bool setTestDay = false;

            foreach (var item in list)
            {
                JToken? main = item["main"];
                JToken? weather = item["weather"]?.FirstOrDefault();
                if (item["dt_txt"] == null || main == null || main["temp_min"] == null || main["temp_max"] == null || main["humidity"] == null || weather == null || weather["id"] == null)
                {
                    throw new Exception($"Missing data in API response at {item}");
                }
                DateTime forecastDate = DateTime.Parse((string)item["dt_txt"]!);
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
            if (simulate)
            {
                responseBody = GetTestJSON("openweather_test.json");
                DateTime date = DateTime.Now;
                responseBody = responseBody.Replace("2022-08-30", date.ToString("yyyy-MM-dd"));

                date = DateTime.Now.AddDays(2);
                responseBody = responseBody.Replace("2022-09-04", date.ToString("yyyy-MM-dd"));
                CountRequest(); // Important: this counts the requests for the limit.
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{_baseURL}forecast?lat={location.Latitude}&lon={location.Longitude}&appid={_apiKey}&units=metric";
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
                            Source = Name
                        };
                    }
                    CountRequest(); // Important: this counts the requests for the limit.
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            Debug.WriteLine(responseBody);
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
                Source = Name,
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
                        if (id == 701)
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
                        if (id == 731)
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
                        Debug.WriteLine("Unknown weather code: " + id);
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

        /// <summary>
        /// Retrieve the current weather of a location
        /// </summary>
        /// <param name="location">A specified location</param>
        /// <param name="simulate">True to simulate data, false to retrieve actual data</param>
        /// <returns>Returns an API response containing a WeatherDataModel depending on the outcome of the executed method</returns>
        public async Task<APIResponse<WeatherDisplayItem>> GetCurrentWeatherAsync(LocationModel location, bool simulate = false)
        {
            try
            {
                if (HasReachedRequestLimit())
                {
                    return new APIResponse<WeatherDisplayItem>
                    {
                        Success = false,
                        ErrorMessage = "Request limit reached\nTo reset change the value in weatherAppData.json in your Documents folder,\nor delete that file.",
                        Source = Name
                    };
                }

                string responseBody;
                if (simulate)
                {
                    responseBody = GetTestJSON("openweather_test.json");
                    //CountRequest(); // Important: this counts the requests for the limit.
                }
                else
                {
                    using HttpClient client = new();
                    string url = $"{_baseURL}weather?lat={location.Latitude}&lon={location.Longitude}&appid={_apiKey}&units=metric";
                    Debug.WriteLine(url);
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                        var errorResponse = JObject.Parse(responseBody);
                        string errorCode = errorResponse["cod"]?.ToString() ?? "Unknown Code";
                        string errorMessage = errorResponse["message"]?.ToString() ?? "Unknown Error";
                        
                        return new APIResponse<WeatherDisplayItem>
                        {
                            Success = false,
                            ErrorMessage = $"{errorCode} - {errorMessage}",
                            Source = Name
                        };
                    }

                    //CountRequest(); // Important: this counts the requests for the limit.
                    responseBody = await response.Content.ReadAsStringAsync();
                }

                JObject weatherResponse = JObject.Parse(responseBody);

                // Check if the necessary data exists in the response
                JToken? main = weatherResponse["main"];
                JToken? weather = weatherResponse["weather"]?.FirstOrDefault();

                if (main == null || weather == null || weather["id"] == null)
                {
                    return new APIResponse<WeatherDisplayItem>
                    {
                        Success = false,
                        ErrorMessage = "Missing weather data in API response",
                        Source = Name
                    };
                }

                // Parse the current weather data
                int weatherId = (int)weather["id"]!;
                WeatherCondition condition = CalculateWeatherCondition(weatherId);

                // Prepare the WeatherInfo string
                string weatherInfo = $"Time: {DateTime.Now.ToString("HH:mm")}, Min Temp: {main["temp_min"]}°C, Max Temp: {main["temp_max"]}°C, Humidity: {main["humidity"]}, Condition: {condition}";

                WeatherDisplayItem currentWeather = new(
                    image: null,
                    weatherInfo: weatherInfo,
                    isDayItem: true
                );

                return new APIResponse<WeatherDisplayItem>
                {
                    Success = true,
                    Source = Name,
                    Data = currentWeather
                };
            }
            catch (Exception ex)
            {
                return new APIResponse<WeatherDisplayItem>
                {
                    Success = false,
                    ErrorMessage = "An error occurred: " + ex.Message,
                    Source = Name
                };
            }
        }
    }
}
