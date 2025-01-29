using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    public class WeatherAPI : WeatherService
    {
        public WeatherAPI() : base("WeatherAPI", "https://api.weatherapi.com/v1/", 1000, -1) // Pas aan op basis van de API-limieten.
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
            Debug.WriteLine($"Requesting day data for {Name}.");
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached. Reset your request count or check the API settings.",
                    Source = Name
                };
            }

            string responseBody;

            using (HttpClient client = new())
            {
                var latitude = location.Latitude.ToString().Replace(",", ".");
                var longitude = location.Longitude.ToString().Replace(",", ".");
                string url = $"{_baseURL}forecast.json?key={_apiKey}&q={latitude},{longitude}&dt={day:yyyy-MM-dd}";
                Debug.WriteLine("URL: " + url);
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
                        Source = Name
                    };
                }
                responseBody = await response.Content.ReadAsStringAsync();
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
                Source = Name,
                Data = weatherData
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
            Debug.WriteLine($"Requesting week data for {Name}.");
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached. Reset your request count or check the API settings.",
                    Source = Name
                };
            }

            string responseBody;

            using (HttpClient client = new())
            {
                var latitude = location.Latitude.ToString().Replace(",", ".");
                var longitude = location.Longitude.ToString().Replace(",", ".");
                string url = $"{_baseURL}forecast.json?key={_apiKey}&q={latitude},{longitude}&days=7";
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
                        Source = Name
                    };
                }
                responseBody = await response.Content.ReadAsStringAsync();
            }

            Debug.WriteLine(responseBody);
            JObject weatherResponse = JObject.Parse(responseBody);

            var forecastDays = weatherResponse["forecast"]?["forecastday"] ?? throw new Exception("Missing forecast data in API response.");
            var weatherData = new List<WeatherDataModel>();

            foreach (var day in forecastDays)
            {
                var condition = CalculateWeatherCondition(day["day"]?["condition"]?["text"]?.ToString());
                var forecastDate = DateTime.Parse(day["date"]?.ToString()!);

                double humidity = day["day"]?["humidity"] != null ? (double)day["day"]?["humidity"] : 0;

                weatherData.Add(new WeatherDataModel(
                    condition,
                    forecastDate,
                    minTemperature: (double)day["day"]?["mintemp_c"]!,
                    maxTemperature: (double)day["day"]?["maxtemp_c"]!,
                    humidity: humidity
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
