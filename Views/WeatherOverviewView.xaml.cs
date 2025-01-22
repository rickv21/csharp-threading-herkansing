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
        MapWebView.Source = "map.html";
    }

    protected override async void OnAppearing()
    {
        Debug.WriteLine("Appear");
        base.OnAppearing();
        if (BindingContext is WeatherOverviewViewModel viewModel) {
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
        throw new NotImplementedException();
    }

    public void OnDayWeekButtonClicked(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

}