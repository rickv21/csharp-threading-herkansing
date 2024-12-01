using System.Collections.ObjectModel;
using WeatherApp.ViewModels;
using WeatherApp.Models;

namespace WeatherApp.Views
{
    public partial class CityView : ContentPage
    {
        private LocationViewModel _viewModel;

        public CityView()
        {
            InitializeComponent();
            _viewModel = new LocationViewModel();
            BindingContext = _viewModel;
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = e.NewTextValue;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var response = await _viewModel.GetLocationAsync(searchQuery);

                if (response.Success)
                {
                    _viewModel.SearchResults = new ObservableCollection<LocationModel>(response.Data);
                }
                else
                {
                    await DisplayAlert("Error", response.ErrorMessage, "OK");
                }
            }
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is LocationModel selectedLocation)
            {
                SaveSelectedLocation(selectedLocation);
            }
        }

        //TODO
        private void SaveSelectedLocation(LocationModel selectedLocation)
        {
            
        }
    }
}