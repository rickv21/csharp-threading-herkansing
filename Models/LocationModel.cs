namespace WeatherApp.Models
{
    /// <summary>
    /// This class represents a location with its name and associated weather data.
    /// </summary>
    /// <param name="name">The name of the location.</param>
    /// <param name="state">The state of the location, optional.</param>
    /// <param name="latitude">The latitude of the location.</param>
    /// <param name="longitude">The longitude of the location.</param>
    /// <param name="weatherData">A list of WeatherDataModels, optional.</param>
    public class LocationModel(string name, string state, string country, string placeId, double latitude, double longitude, List<WeatherDataModel>? weatherData = null)
    {

        /// <summary>
        /// The name of the location.
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// The state of the location.
        /// </summary>
        public string? State { get; set; } = state;

        /// <summary>
        /// The country of the location.
        /// </summary>
        public string? Country { get; set; } = country;

        /// <summary>
        /// The place id of the location.
        /// </summary>
        public string? PlaceId { get; set; } = placeId;

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

        public string ToString()
        {
            return Name;
        }
    }
}
