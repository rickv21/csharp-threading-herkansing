using WeatherApp.ViewModels;
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
        BindingContext = new WeatherOverviewViewModel();
        MapWebView.Source = "map.html";
    }

    private void OnRightButtonClicked(object sender, EventArgs e)
    {
        var viewModel = BindingContext as WeatherOverviewViewModel;
        viewModel.OnRightButtonClicked(this, null);
    }
    private void OnExportButtonClicked(object sender, EventArgs e)
    {
        var viewModel = BindingContext as WeatherOverviewViewModel;
        viewModel.Export();
    }

    protected override async void OnAppearing()
    {
        Debug.WriteLine("Appear");
        base.OnAppearing();
        if (BindingContext is WeatherOverviewViewModel viewModel) {
        }
    }

    protected override void OnDisappearing()
    {
        Debug.WriteLine("Disappear");
        base.OnDisappearing();
    }
}