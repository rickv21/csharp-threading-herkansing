﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WeatherApp.Utils;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Models
{
    /// <summary>
    /// This class represents the weather data for a specific time.
    /// </summary>
    public class WeatherDataModel(WeatherCondition condition, DateTime timeStamp, double minTemperature, double maxTemperature, double humidity)
    {
        /// <summary>
        /// The weather condition (e.g., sunny, cloudy, etc.).
        /// </summary>
        /// 
        [JsonConverter(typeof(StringEnumConverter))]
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

        //Computed Properties for UI Binding
        public string ConditionFormatted => WeatherUtils.TranslateWeatherCondition(condition);
        [JsonIgnore]
        public string MinTemperatureFormatted => $"Min. Temp: {MinTemperature}°C";
        [JsonIgnore]
        public string MaxTemperatureFormatted => $"Max. Temp: {MaxTemperature}°C";
        [JsonIgnore]
        public string HumidityFormatted => $"Luchtvochtigheid: {Math.Round(Humidity, 2)}%";

        /// <summary>
        /// Returns a string that represents the weather data.
        /// </summary>
        /// <returns>A string that represents the weather data.</returns>
        public override string ToString()
        {
            return $"Condition: {Condition}, Time: {TimeStamp}, Min Temp: {MinTemperature}, Max Temp: {MaxTemperature}, Humidity: {Humidity}";
        }
    }
}
