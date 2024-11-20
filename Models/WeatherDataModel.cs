using WeatherApp.WeatherAPIs;

namespace WeatherApp.Models
{
    public class WeatherDataModel(WeatherCondition condition, DateTime timeStamp, double minTemperature, double maxTemperature, double humidity)
    {
        public WeatherCondition Condition { get; } = condition;
        public DateTime TimeStamp { get; } = timeStamp;
        public double MinTemperature { get; } = minTemperature;
        public double MaxTemperature { get; } = maxTemperature;
        public double Humidity { get; } = humidity;

        public override string ToString()
        {
            return $"Condition: {Condition}, Time: {TimeStamp}, Min Temp: {MinTemperature}°C, Max Temp: {MaxTemperature}°C, Humidity: {Humidity}%";
        }
    }
}
