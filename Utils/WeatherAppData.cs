using WeatherApp.Models;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Utils
{
    /// <summary>
    /// Contains data of the application that can be accessed from any page (singleton).
    /// </summary>
    public class WeatherAppData
    {
        public Dictionary<string, WeatherService> WeatherServices { get; set; } = [];
        public Dictionary<int, List<WeatherDataModel>> WeatherCache { get; set; } = [];
        public List<LocationModel> Locations { get; set; } = [];
    }
}
