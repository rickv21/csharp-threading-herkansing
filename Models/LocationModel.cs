using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Models
{
    public class LocationModel(string name, List<WeatherDataModel> weatherData)
    {
        public string Name { get; set; } = name;
        public List<WeatherDataModel> WeatherData { get; set; } = weatherData;
    }
}
