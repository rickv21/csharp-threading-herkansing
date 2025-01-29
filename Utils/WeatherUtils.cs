using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Utils
{
    public class WeatherUtils
    {
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
                WeatherCondition.HAZE => "Wazig",
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
