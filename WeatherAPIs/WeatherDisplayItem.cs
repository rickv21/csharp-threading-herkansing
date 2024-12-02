namespace WeatherApp.WeatherAPIs
{
    public class WeatherDisplayItem(ImageSource image, string weatherInfo)
    {
        public ImageSource Image { get; set; } = image;

        public string WeatherInfo { get; set; } = weatherInfo;
    }

}
