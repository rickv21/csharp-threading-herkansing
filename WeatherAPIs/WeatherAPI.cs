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
                responseBody = GetTestJSON("weather_api_test.json");
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
            string condition = ((string)data).Trim().ToLower(); //Deletes space and lowercase

            switch (condition)
            {
                case "partly cloudy":
                    return WeatherCondition.PARTLY_CLOUDY;
                case "sunny":
                    return WeatherCondition.SUNNY;
                case "cloudy":
                    return WeatherCondition.CLOUDY;
                case "clear":
                    return WeatherCondition.CLEAR;
                case "patchy rain nearby":
                    return WeatherCondition.RAIN;
                case "overcast":
                    return WeatherCondition.CLOUDY;
                case "mist":
                    return WeatherCondition.MIST;
                case "patchy rain possible":
                    return WeatherCondition.DRIZZLE;
                case "patchy snow possible":
                    return WeatherCondition.SNOW;
                case "patchy sleet possible":
                    return WeatherCondition.SNOW;
                case "patchy freezing drizzle possible":
                    return WeatherCondition.DRIZZLE;
                case "thundery outbreaks possible":
                    return WeatherCondition.THUNDERSTORM;
                case "blowing snow":
                    return WeatherCondition.SNOW;
                case "blizzard":
                    return WeatherCondition.SNOW;
                case "fog":
                    return WeatherCondition.FOG;
                case "freezing fog":
                    return WeatherCondition.FOG;
                case "patchy light drizzle":
                    return WeatherCondition.DRIZZLE;
                case "light drizzle":
                    return WeatherCondition.DRIZZLE;
                case "freezing drizzle":
                    return WeatherCondition.DRIZZLE;
                case "heavy freezing drizzle":
                    return WeatherCondition.DRIZZLE;
                case "patchy light rain":
                    return WeatherCondition.RAIN;
                case "light rain":
                    return WeatherCondition.RAIN;
                case "moderate rain at times":
                    return WeatherCondition.RAIN;
                case "moderate rain":
                    return WeatherCondition.RAIN;
                case "heavy rain at times":
                    return WeatherCondition.RAIN;
                case "heavy rain":
                    return WeatherCondition.RAIN;
                case "light freezing rain":
                    return WeatherCondition.RAIN;
                case "moderate or heavy freezing rain":
                    return WeatherCondition.RAIN;
                case "light sleet":
                    return WeatherCondition.SNOW;
                case "moderate or heavy sleet":
                    return WeatherCondition.SNOW;
                case "patchy light snow":
                    return WeatherCondition.SNOW;
                case "light snow":
                    return WeatherCondition.SNOW;
                case "patchy moderate snow":
                    return WeatherCondition.SNOW;
                case "moderate snow":
                    return WeatherCondition.SNOW;
                case "patchy heavy snow":
                    return WeatherCondition.SNOW;
                case "heavy snow":
                    return WeatherCondition.SNOW;
                case "ice pellets":
                    return WeatherCondition.HAIL;
                case "light rain shower":
                    return WeatherCondition.RAIN;
                case "moderate or heavy rain shower":
                    return WeatherCondition.RAIN;
                case "torrential rain shower":
                    return WeatherCondition.RAIN;
                case "light sleet showers":
                    return WeatherCondition.SNOW;
                case "moderate or heavy sleet showers":
                    return WeatherCondition.SNOW;
                case "light snow showers":
                    return WeatherCondition.SNOW;
                case "moderate or heavy snow showers":
                    return WeatherCondition.SNOW;
                case "light showers of ice pellets":
                    return WeatherCondition.HAIL;
                case "moderate or heavy showers of ice pellets":
                    return WeatherCondition.HAIL;
                case "patchy light rain with thunder":
                    return WeatherCondition.THUNDERSTORM;
                case "moderate or heavy rain with thunder":
                    return WeatherCondition.THUNDERSTORM;
                case "patchy light snow with thunder":
                    return WeatherCondition.THUNDERSTORM;
                case "moderate or heavy snow with thunder":
                    return WeatherCondition.THUNDERSTORM;
            }

            Debug.WriteLine($"Unknown condition: '{condition}'");
            return WeatherCondition.UNKNOWN;
        }
    }
}
