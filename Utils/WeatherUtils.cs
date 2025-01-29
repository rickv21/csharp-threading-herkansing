using System.Globalization;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Utils
{
    /// <summary>
    /// Provides utility functions for weather-related operations.
    /// </summary>
    public class WeatherUtils
    {
        /// <summary>
        /// Translates a given <see cref="DayOfWeek"/> to Dutch.
        /// </summary>
        /// <param name="day">The day of the week.</param>
        /// <returns>The Dutch translation of the day.</returns>
        public static string TranslateDayOfTheWeek(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Maandag",
                DayOfWeek.Tuesday => "Dinsdag",
                DayOfWeek.Wednesday => "Woensdag",
                DayOfWeek.Thursday => "Donderdag",
                DayOfWeek.Friday => "Vrijdag",
                DayOfWeek.Saturday => "Zaterdag",
                DayOfWeek.Sunday => "Zondag",
                _ => "Onbekend",
            };
        }

        /// <summary>
        /// Translates a <see cref="WeatherCondition"/> enum to Dutch.
        /// </summary>
        /// <param name="condition">The weather condition.</param>
        /// <returns>The Dutch translation of the weather condition.</returns>
        public static string TranslateWeatherCondition(WeatherCondition condition)
        {
            return condition switch
            {
                WeatherCondition.SUNNY => "Zonnig",
                WeatherCondition.RAIN => "Regen",
                WeatherCondition.CLOUDY => "Bewolkt",
                WeatherCondition.THUNDERSTORM => "Onweer",
                WeatherCondition.SNOW => "Sneeuw",
                WeatherCondition.PARTLY_CLOUDY => "Gedeeltelijk bewolkt",
                WeatherCondition.HAIL => "Hagel",
                WeatherCondition.MIST => "Mist",
                WeatherCondition.STORMY => "Stormachtig",
                WeatherCondition.WINDY => "Winderig",
                WeatherCondition.DRIZZLE => "Motregen",
                WeatherCondition.FOG => "Dichte mist",
                WeatherCondition.HAZE => "Nevel",
                WeatherCondition.DUST => "Stof",
                WeatherCondition.ASH => "As",
                WeatherCondition.SQUALL => "Vlaag",
                WeatherCondition.TORNADO => "Tornado",
                WeatherCondition.SAND => "Zand",
                WeatherCondition.SMOKE => "Rook",
                WeatherCondition.CLEAR => "Helder",
                WeatherCondition.COLD => "Koud",
                WeatherCondition.ICE => "IJs",
                WeatherCondition.UNKNOWN => "Onbekend",
                _ => "Onbekende conditie",
            };
        }
    }
}
