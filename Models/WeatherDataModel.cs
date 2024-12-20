using WeatherApp.WeatherAPIs;

namespace WeatherApp.Models
{
    /// <summary>
    /// This class represents the weather data for a specific time.
    /// </summary>
    public class WeatherDataModel(string apiSource, WeatherCondition condition, DateTime timeStamp, double minTemperature, double maxTemperature, double humidity)
    {
        /// <summary>
        /// The name of the API that this data was obtained from.
        /// </summary>
        public string APISource { get; } = apiSource;

        /// <summary>
        /// The weather condition (e.g., sunny, cloudy, etc.).
        /// </summary>
        public WeatherCondition Condition { get; } = condition;

        /// <summary>
        /// The date and time when the weather data was recorded.
        /// </summary>
        public DateTime TimeStamp { get; } = timeStamp;

        /// <summary>
        /// The minimum temperature recorded.
        /// </summary>
        public double MinTemperature { get; } = minTemperature;

        /// <summary>
        /// The maximum temperature recorded.
        /// </summary>
        public double MaxTemperature { get; } = maxTemperature;

        /// <summary>
        /// The humidity level recorded.
        /// </summary>
        public double Humidity { get; } = humidity;

        /// <summary>
        /// Returns a string that represents the weather data.
        /// </summary>
        /// <returns>A string that represents the weather data.</returns>
        public override string ToString()
        {
            return $"Source: {APISource}, Condition: {Condition}, Time: {TimeStamp}, Min Temp: {MinTemperature}°C, Max Temp: {MaxTemperature}°C, Humidity: {Humidity}%";
        }
    }
}
