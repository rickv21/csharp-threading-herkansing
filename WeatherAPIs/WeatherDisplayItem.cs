using System.Text.Json.Serialization;

namespace WeatherApp.WeatherAPIs
{
    public class WeatherDisplayItem
    {
        [JsonIgnore] //To ignore imagesource for json export
        public ImageSource? Image { get; set; }
        public string WeatherInfo { get; set; }
        public string TimeStamp { get; set; }
        public string MinTemp { get; set; }
        public string MaxTemp { get; set; }
        public string Humidity { get; set; }
        public string Condition { get; set; }
        public string DisplayText => ToString();

        public WeatherDisplayItem(ImageSource image, string weatherInfo, bool isDayItem)
        {
            Image = image;
            WeatherInfo = weatherInfo;

            // Parse weatherInfo string
            if (!string.IsNullOrEmpty(weatherInfo))
            {
                var parts = weatherInfo.Split(',');
                if (!isDayItem)
                {
                    TranslateDaysOfTheWeek(WeatherInfo.Split(' ')[0]);
                }
                foreach (var part in parts)
                {
                    if (part.Contains("Time:"))
                    {
                        if (isDayItem)
                        {
                            TimeStamp = part.Substring(part.Length - 8, 5);
                        }
                    }
                    else if (part.Contains("Min Temp:"))
                    {
                        MinTemp = "Min. Temp: " + part.Split(':')[1].Trim() + " °C";
                    }
                    else if (part.Contains("Max Temp:"))
                    {
                        MaxTemp = "Max. Temp: " + part.Split(':')[1].Trim() + " °C";
                    }
                    else if (part.Contains("Humidity:"))
                    {
                        Humidity = "Luchtvochtigheid: " + part.Split(':')[1].Trim();
                    }
                    else if (part.Contains("Condition:"))
                    {
                        switch (part.Split(':')[1].Trim())
                        {
                            case "SUNNY":
                                Condition = "Zonnig";
                                break;
                            case "RAIN":
                                Condition = "Regen";
                                break;
                            case "CLOUDY":
                                Condition = "Bewolkt";
                                break;
                            case "THUNDERSTORM":
                                Condition = "Onweer";
                                break;
                            case "SNOW":
                                Condition = "Sneeuw";
                                break;
                            case "PARTLY_CLOUDY":
                                Condition = "Gedeeltelijk bewolkt";
                                break;
                            case "HAIL":
                                Condition = "Hagel";
                                break;
                            case "MIST":
                                Condition = "Mist";
                                break;
                            case "STORMY":
                                Condition = "Stormachtig";
                                break;
                            case "WINDY":
                                Condition = "Winderig";
                                break;
                            case "DRIZZLE":
                                Condition = "Motregen";
                                break;
                            case "FOG":
                                Condition = "Dichte mist";
                                break;
                            case "HAZE":
                                Condition = "Wazig";
                                break;
                            case "DUST":
                                Condition = "Stof";
                                break;
                            case "ASH":
                                Condition = "As";
                                break;
                            case "SQUALL":
                                Condition = "Vlaag";
                                break;
                            case "TORNADO":
                                Condition = "Tornado";
                                break;
                            case "SAND":
                                Condition = "Zand";
                                break;
                            case "SMOKE":
                                Condition = "Rook";
                                break;
                            case "CLEAR":
                                Condition = "Helder";
                                break;
                            case "COLD":
                                Condition = "Koud";
                                break;
                            case "ICE":
                                Condition = "IJs";
                                break;
                            case "UNKNOWN":
                                Condition = "Onbekend";
                                break;
                            default:
                                Condition = "Onbekende conditie";
                                break;
                        }
                    }
                }
            }
        }

        private void TranslateDaysOfTheWeek(string text)
        {
            switch (text)
            {
                case "Monday":
                    TimeStamp =  "Maandag";
                    break;
                case "Tuesday":
                    TimeStamp = "Dinsdag";
                    break;
                case "Wednesday":
                    TimeStamp = "Woensdag";
                    break;
                case "Thursday":
                    TimeStamp = "Donderdag";
                    break;
                case "Friday":
                    TimeStamp = "Vrijdag";
                    break;
                case "Saturday":
                    TimeStamp = "Zaterdag";
                    break;
                case "Sunday":
                    TimeStamp = "Zondag";
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return $"Tijd: {TimeStamp}, Min. Temp: {MinTemp}, Max. Temp: {MaxTemp}, Luchtvochtigheid: {Humidity}, Conditie: {Condition}";
        }
    }
}
