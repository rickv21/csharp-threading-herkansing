using Microsoft.Maui.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    internal class WeerLiveAPI : WeatherService
    {
        public WeerLiveAPI() : base("WeerLive", " https://weerlive.nl/api/weerlive_api_v2.php?key=", 300, -1)
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
            using (HttpClient client = new HttpClient())
            {
                string url = $"{_baseURL}{_apiKey}&locatie=Emmen";
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

                var weatherData = new List<WeatherDataModel>();


                /*weatherData.Add(new WeatherDataModel(
                        condition,
                        forecastDate,
                        minTemperature: (double)main["temp_min"]!,
                        maxTemperature: (double)main["temp_max"]!,
                        humidity: (double)main["humidity"]!
                        ));*/

                return new APIResponse<List<WeatherDataModel>>
                {
                    Success = true,
                    ErrorMessage = null,
                    Data = weatherData
                };
            }

        }

        public override Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(string location, bool simulate = false)
        {
            throw new NotImplementedException();
        }

        protected override WeatherCondition CalculateWeatherCondition(object data)
        {
            throw new NotImplementedException();
        }

        protected override string GetTestJSON()
        {
            throw new NotImplementedException();
        }
    }
}
