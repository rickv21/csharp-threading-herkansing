using System.Text.Json.Serialization;

namespace WeatherApp.Models
{
    /// <summary>
    /// Represents a weather display model used for UI presentation.
    /// </summary>
    public class WeatherDisplayModel(ImageSource image, WeatherDataModel weatherData, string localizedName)
    {
        /// <summary>
        /// Weather condition icon
        /// </summary>
        [JsonIgnore] //To ignore imagesource for json export
        public ImageSource Image { get; set; } = image;
        /// <summary>
        /// The weather data to be displayed.
        /// </summary>
        public WeatherDataModel WeatherData { get; set; } = weatherData;
        /// <summary>
        /// The localized name of the weather condition.
        /// </summary>
        public string LocalizedName { get; set; } = localizedName;
        public string DisplayText => ToString();

        public override string ToString()
        {
            return $"Tijd: {WeatherData.TimeStamp}, Min. Temp: {WeatherData.MinTemperature}, Max. Temp: {WeatherData.MaxTemperature}, Luchtvochtigheid: {WeatherData.Humidity}, Conditie: {WeatherData.Condition}, Vertaalde conditie: {WeatherData.ConditionFormatted}";
        }
    }
}
