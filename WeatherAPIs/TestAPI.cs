using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    public class TestAPI : WeatherService
    {
        public TestAPI() : base("Test", "https://jsonplaceholder.typicode.com", 5, 10)
        {
        }
        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location, bool simulate = false)
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
            if (simulate)
            {
                CountRequest(); // TODO: Do we want to count if we are simulating??
                var weatherData = new List<WeatherDataModel>
                {
                    new WeatherDataModel(
                        WeatherCondition.SUNNY,
                        day,
                        minTemperature: 15.0,
                        maxTemperature: 25.0,
                        humidity: 20.0
                    )
                };

                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = true,
                    ErrorMessage = null,
                    Data = weatherData
                };
            }
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{_baseURL}/posts/1");
                response.EnsureSuccessStatusCode();
                CountRequest(); // Important: this counts the requests for the limit.

                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Write(responseBody);
                JObject post = JObject.Parse(responseBody);
                int id = (int?)post["id"] ?? -1; //Fallback to -1 if not found in response.

                // Creating dummy weather data using the response
                var weatherData = new List<WeatherDataModel>
                {
                    new WeatherDataModel(
                        WeatherCondition.SUNNY,
                        day,
                        minTemperature: 15.0,
                        maxTemperature: 25.0,
                        humidity: id
                    )
                };

                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = true,
                    ErrorMessage = null,
                    Data = weatherData
                };
            }
        }

        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(LocationModel location, bool simulate = false)
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
                responseBody = GetTestJSON("accu_weather_hour_test.json");
                CountRequest(); // TODO: Do we want to count if we are simulating??

            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
                    response.EnsureSuccessStatusCode();
                    CountRequest(); // Important: this counts the requests for the limit.

                    responseBody = await response.Content.ReadAsStringAsync();

                }
            }
            Debug.Write(responseBody);
            JObject post = JObject.Parse(responseBody);

            // Creating dummy weather data for a week
            var weatherData = new List<WeatherDataModel>();
            for (int i = 0; i < 7; i++)
            {
                weatherData.Add(new WeatherDataModel(
                    WeatherCondition.SUNNY,
                    DateTime.Now.AddDays(i),
                    minTemperature: 15.0,
                    maxTemperature: 25.0,
                    humidity: 50.0
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
            return WeatherCondition.SUNNY;
        }
    }
}
