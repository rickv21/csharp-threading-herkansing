namespace WeatherApp.Models
{
    public class LocationModel(string name, List<WeatherDataModel> weatherData)
    {
        public string Name { get; set; } = name;
        public List<WeatherDataModel> WeatherData { get; set; } = weatherData;
    }
}
