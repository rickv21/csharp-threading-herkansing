using System.Diagnostics;
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
        base.OnAppearing();
        if (BindingContext is WeatherOverviewViewModel viewModel)
        {
            viewModel.SetDefaultViewData();
        }
    }

    protected override void OnDisappearing()
    {
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
