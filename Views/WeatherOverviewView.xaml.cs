using WeatherApp.ViewModels;

namespace WeatherApp.Views;

public partial class WeatherOverviewView : ContentPage
{
	public WeatherOverviewView()
	{
		InitializeComponent();
        MapWebView.Source = "map.html";
        BindingContext = new WeatherOverviewViewModel();
    }
}