using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Models
{
    public class WeatherDataModel(WeatherCondition condition, DateTime timeStamp, double minTemperature, double maxTemperature, double humidity)
    {
        public WeatherCondition Condition { get; } = condition;
        public DateTime TimeStamp { get; } = timeStamp;
        public double MinTemperature { get; } = minTemperature;
        public double MaxTemperature { get; } = maxTemperature;
        public double Humidity { get; } = humidity;
    }
}
