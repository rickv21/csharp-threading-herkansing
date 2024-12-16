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

    private void OnRightButtonClicked(object sender, EventArgs e)
    {
        var viewModel = BindingContext as WeatherOverviewViewModel;
        viewModel.OnRightButtonClicked(this, null);
    }
    private void OnExportButtonClicked(object sender, EventArgs e)
    {
        var viewModel = BindingContext as WeatherOverviewViewModel;
        viewModel.OnExportButtonClicked(this, null);
    }
}