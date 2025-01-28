using System.Diagnostics;
using Microsoft.Maui.Controls;
using WeatherApp.Utils;
using WeatherApp.ViewModels;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Views;

public partial class WeatherOverviewView : ContentPage
{
    private readonly string _apiKey;
    public WeatherOverviewView()
    {
        InitializeComponent();
        BindingContext = new WeatherOverviewViewModel(App.Current.Handler.MauiContext.Services.GetService<WeatherAppData>());

        _apiKey = new OpenWeatherMapAPI().OpenWeatherApiKey;

        // Kaart
        string htmlContent = @"
        <!DOCTYPE html>
        <html>
        <head>
            <link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.3/dist/leaflet.css"" />
            <script src=""https://unpkg.com/leaflet@1.9.3/dist/leaflet.js""></script>
        </head>
        <body>
            <div id=""map"" style=""width: 100%; height: 100vh;""></div>
            <script>
               var map = L.map('map', {
                    center: [52.1326, 5.2913], // Center of the Netherlands
                    zoom: 7, // Initial zoom level
                    minZoom: 7, // Minimum zoom level (zoom out (restricted))
                    maxZoom: 19, // Maximum zoom level (zoom in)
                    maxBounds: [[50.7504, 3.3584], [53.6316, 7.2275]], // Southwest and northeast bounds of NL
                    maxBoundsViscosity: 0.5,
                    wheelPxPerZoomLevel: 60,
                });

                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: 'Map data � <a href=""https://www.openstreetmap.org/"">OpenStreetMap</a> contributors',
                    maxZoom: 19,
                }).addTo(map);

                L.tileLayer('https://tile.openweathermap.org/map/clouds/{z}/{x}/{y}.png?appid=8c68dd6ad84040c07b526c2be1059600', {
                    attribution: 'Cloud data � <a href=""https://openweathermap.org/"">OpenWeatherMap</a>',
                    maxZoom: 19,
                }).addTo(map);
            </script>
        </body>
        </html>";

        MapWebView.Source = new HtmlWebViewSource
        {
            Html = htmlContent
        };
    }

    protected override async void OnAppearing()
    {
        Debug.WriteLine("Appear");
        base.OnAppearing();
        if (BindingContext is WeatherOverviewViewModel viewModel)
        {
            viewModel.UpdateGUI();
        }
    }

    protected override void OnDisappearing()
    {
        Debug.WriteLine("Disappear");
        base.OnDisappearing();
    }

    public void OnExportButtonClicked(object sender, EventArgs e)
    {
        if (BindingContext is WeatherOverviewViewModel viewModel)
        {
            viewModel.Export();
        }
    }

    public void OnDayWeekButtonClicked(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void CityViewButton_OnClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CityView());
    }
}
