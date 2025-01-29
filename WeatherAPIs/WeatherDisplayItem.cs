using System.Text.Json.Serialization;
using WeatherApp.Models;

namespace WeatherApp.WeatherAPIs
{
    public class WeatherDisplayItem(ImageSource image, WeatherDataModel weatherData, string localizedName)
    {
        [JsonIgnore] //To ignore imagesource for json export
        public ImageSource Image { get; set; } = image;
        public WeatherDataModel WeatherData { get; set; } = weatherData;
        public string LocalizedName { get; set; } = localizedName;
        public string DisplayText => ToString();

        public override string ToString()
        {
            return $"Tijd: {WeatherData.TimeStamp}, Min. Temp: {WeatherData.MinTemperature}, Max. Temp: {WeatherData.MaxTemperature}, Luchtvochtigheid: {WeatherData.Humidity}, Conditie: {WeatherData.Condition}";
        }
    }
}
