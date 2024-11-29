namespace WeatherApp.Models
{
    /// <summary>
    /// This class represents a location with its name and associated weather data.
    /// </summary>
    /// <param name="name">The name of the location.</param>
    /// <param name="latitude">The latitude of the location.</param>
    /// <param name="longitude">The longitude of the location.</param>
    /// <param name="weatherData">A list of WeatherDataModels, optional.</param>
    public class LocationModel(string name, double latitude, double longitude, List<WeatherDataModel>? weatherData = null)
    {

        /// <summary>
        /// The name of the location.
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// The latitude of the location.
        /// </summary>
        public double Latitude { get; set; } = latitude;

        /// <summary>
        /// The longitude of the location.
        /// </summary>
        public double Longitude { get; set; } = longitude;

        /// <summary>
        /// A list of WeatherDataModels.
        /// This either represents multiple hours of a day or multiple days of a week.
        /// </summary>
        public List<WeatherDataModel> WeatherData { get; set; } = weatherData ?? [];
    }
}
