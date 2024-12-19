using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Utils
{
    public class WeatherAppData
    {
        public List<WeatherService> WeatherServices { get; set; }
        public Dictionary<int, List<Models.WeatherDataModel>> HourlyData { get; set; }
        public bool SimulateMode { get; set; }

        public WeatherAppData()
        {
            WeatherServices = new List<WeatherService>();
            HourlyData = new Dictionary<int, List<Models.WeatherDataModel>>();
            SimulateMode = false;
        }
    }

}
