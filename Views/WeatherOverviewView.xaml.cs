using System.Diagnostics;
using Microsoft.Maui.Controls;
using WeatherApp.Utils;
using WeatherApp.ViewModels;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Views;

public partial class WeatherOverviewView : ContentPage
{
    public WeatherOverviewView()
    {
        InitializeComponent();
        BindingContext = new WeatherOverviewViewModel(App.Current.Handler.MauiContext.Services.GetService<WeatherAppData>());

        string mapCode = new OpenWeatherMapAPI().GetMapCode();
        MapWebView.Source = new HtmlWebViewSource
        {
            Html = mapCode
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

    private void CityViewButton_OnClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CityView());
    }
}
