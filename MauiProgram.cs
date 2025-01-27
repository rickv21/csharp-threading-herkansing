using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using WeatherApp.Utils;
using WeatherApp.WeatherAPIs;

namespace WeatherApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<WeatherAppData>(sp =>
            {
                var data = new WeatherAppData();
                InitializeWeatherAppData(data);
                return data;
            });



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void InitializeWeatherAppData(WeatherAppData appData)
        {
            JsonFileManager jsonManager = new JsonFileManager();

            // Retrieve the simulateData boolean.
            var data = jsonManager.GetData("data", "simulateMode") as string;

            if (bool.TryParse(data, out bool isEnabled))
            {
                appData.SimulateMode = isEnabled;
            }

            //TEMP
            appData.Locations.Add(new("Emmen", "Drenthe", "NL", "Test", 52.787701, 6.894810, null));
            appData.Locations.Add(new("Amsterdam", "Noord-Holland", "NL", "Test", 52.377956, 4.897070, null));

            try
            {
                // Add supported weather services
                appData.WeatherServices.Add("Open Weather Map", new OpenWeatherMapAPI());
                appData.WeatherServices.Add("AccuWeather", new AccuWeatherAPI());
                appData.WeatherServices.Add("WeerLive", new WeerLiveAPI());
                appData.WeatherServices.Add("WeatherAPI", new WeatherAPI());
                appData.WeatherServices.Add("Weatherbit", new WeatherbitAPI());
                appData.WeatherServices.Add("Visual Crossing", new VisualCrossingAPI());
            }
            catch (Exception ex)
            {
                Shell.Current.DisplayAlert("Error loading API", ex.Message, "OK");
                Debug.WriteLine($"Error loading API: {ex}");
            }
        }
    }
}
