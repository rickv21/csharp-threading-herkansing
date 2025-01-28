using System.Collections.ObjectModel;
using System.ComponentModel;
using WeatherApp.WeatherAPIs;

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
    public class LocationModel(string name, string state, string country, string placeId, double latitude, double longitude, ObservableCollection<WeatherDisplayItem>? weatherData = null) : INotifyPropertyChanged
    {
        /// <summary>
        /// The name of the location
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// The state of the location
        /// </summary>
        public string? State { get; set; } = state;

        /// <summary>
        /// The country of the location
        /// </summary>
        public string? Country { get; set; } = country;

        /// <summary>
        /// The place id of the location
        /// </summary>
        public string PlaceId { get; set; } = placeId;

        /// <summary>
        /// The latitude of the location.
        /// </summary>
        public double Latitude { get; set; } = latitude;

        /// <summary>
        /// The longitude of the location
        /// </summary>
        public double Longitude { get; set; } = longitude;

        public bool IsSelected { get; set; } = false;

        /// <summary>
        /// An ObservableCollection of WeatherDataModels
        /// This either represents multiple hours of a day or multiple days of a week
        /// </summary>
        private ObservableCollection<WeatherDisplayItem> _weatherData;

        public ObservableCollection<WeatherDisplayItem> WeatherData
        {
            get => _weatherData;
            set
            {
                if (_weatherData != value)
                {
                    _weatherData = value;
                    OnPropertyChanged(nameof(WeatherData));
                }
            }
        }

        /// <summary>
        /// A boolean to check if any weather data is available
        /// </summary>
        private bool _isWeatherDataAvailable = false;

        public bool IsWeatherDataAvailable
        {
            get => _isWeatherDataAvailable;
            set
            {
                _isWeatherDataAvailable = value;
                OnPropertyChanged(nameof(IsWeatherDataAvailable));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
