using System.Diagnostics;
using WeatherApp.ViewModels;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.Views;

public partial class WeatherOverviewView : ContentPage
{
	public WeatherOverviewView()
	{
		InitializeComponent();
        BindingContext = new WeatherOverviewViewModel();
        MapWebView.Source = "map.html";
    }

    public WeatherOverviewView(List<WeatherService> weatherServices, Dictionary<int, List<Models.WeatherDataModel>> hourlyData, bool simulateMode)
    {
        InitializeComponent();
        BindingContext = new WeatherOverviewViewModel(weatherServices, hourlyData, simulateMode);
        MapWebView.Source = "map.html";
    }

    protected override void OnAppearing()
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
}