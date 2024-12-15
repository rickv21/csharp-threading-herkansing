using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    internal class WeatherbitAPI : WeatherService
    {
        public WeatherbitAPI() : base("Weatherbit", "http://api.weatherbit.io/v2.0/forecast/hourly", 50, -1)
        {
        }

        public override Task<APIResponse<List<WeatherDataModel>>> GetWeatherDataAsync(DateTime day, LocationModel location, bool simulate = false)
        {
            throw new NotImplementedException();
        }

        public override Task<APIResponse<List<WeatherDataModel>>> GetWeatherForAWeekAsync(LocationModel location, bool simulate = false)
        {
            throw new NotImplementedException();
        }

        protected override WeatherCondition CalculateWeatherCondition(object data)
        {
            throw new NotImplementedException();
        }
    }
}
