using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    public class TestAPI : WeatherService
    {
        public TestAPI() : base("Test", "https://jsonplaceholder.typicode.com", 5, 10)
        {
        }
    public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, string location)
        {
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached",
                    Data = null
                };
            }
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
                response.EnsureSuccessStatusCode();
                CountRequest(); // Important: this counts the requests for the limit.

                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Write(responseBody);
                JObject post = JObject.Parse(responseBody);

                // Creating dummy weather data using the response
                var weatherData = new List<WeatherDataModel>
                {
                    new WeatherDataModel(
                        WeatherCondition.SUNNY,
                        day,
                        minTemperature: 15.0,
                        maxTemperature: 25.0,
                        humidity: 50.0
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

        public override async Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(string Location)
        {
            if (HasReachedRequestLimit())
            {
                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = false,
                    ErrorMessage = "Request limit reached",
                    Data = null
                };
            }
            using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
            response.EnsureSuccessStatusCode();
            CountRequest(); // Important: this counts the requests for the limit.

            string responseBody = await response.Content.ReadAsStringAsync();
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
    }

}
}
