using Microsoft.Extensions.Logging;
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
            builder.Services.AddSingleton<IAlertService, AlertService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        /// <summary>
        /// Loads the config values and weather services.
        /// </summary>
        /// <param name="appData">The WeatherAppData object to be updated.</param>
        private static void InitializeWeatherAppData(WeatherAppData appData)
        {
            JsonFileManager jsonManager = new JsonFileManager();

            // Retrieve the simulateData boolean.
            var data = jsonManager.GetBoolean(["data", "simulateMode"]) ?? false;
            appData.SimulateMode = data;
            Debug.WriteLine("Loaded SimulateMode: " + data);

            PlacesManager placesManager = new PlacesManager();
            appData.Locations = placesManager.LoadLocationsFromFile();

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
