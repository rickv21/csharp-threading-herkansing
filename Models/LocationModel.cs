namespace WeatherApp.Models
{
    /// <summary>
    /// This class represents a location with its name and associated weather data.
    /// </summary>
    public class LocationModel(string name, List<WeatherDataModel> weatherData)
    {
        /// <summary>
        /// The name of the location.
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// A list of WeatherDataModels.
        /// This either represents multiple hours of a day or multiple days of a week.
        /// </summary>
        public List<WeatherDataModel> WeatherData { get; set; } = weatherData;
    }
}
