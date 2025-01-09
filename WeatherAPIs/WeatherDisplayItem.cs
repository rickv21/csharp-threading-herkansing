namespace WeatherApp.WeatherAPIs
{
    public class WeatherDisplayItem
    {
        public ImageSource Image { get; set; }

        public string WeatherInfo { get; set; }

        public string TimeStamp { get; set; }
        public string MinTemp { get; set; }
        public string MaxTemp { get; set; }
        public string Humidity { get; set; }

        public WeatherDisplayItem(ImageSource image, string weatherInfo)
        {
            Image = image;
            WeatherInfo = weatherInfo;

            // Parse weatherInfo string
            if (!string.IsNullOrEmpty(weatherInfo))
            {
                var parts = weatherInfo.Split(',');

                foreach (var part in parts)
                {
                    if (part.Contains("Time:"))
                    {
                        TimeStamp = part.Substring(part.Length - 8, 5);
                    }
                    else if (part.Contains("Min Temp:"))
                    {
                        MinTemp = "Min Temp: " + part.Split(':')[1].Trim() + " °C";
                    }
                    else if (part.Contains("Max Temp:"))
                    {
                        MaxTemp = "Max Temp: " + part.Split(':')[1].Trim() + " °C";
                    }
                    else if (part.Contains("Humidity:"))
                    {
                        Humidity = "Humidity: " + part.Split(':')[1].Trim();
                    }
                }
            }
        }

    }

}
