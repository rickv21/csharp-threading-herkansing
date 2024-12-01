using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.ViewModels;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class LocationViewModel
    {
        private readonly GeocodingAPI _api;
        private string _searchQuery;
        private Action<string> _onSearchQueryChanged;

        public ObservableCollection<LocationModel> SearchResults { get; set; }
        
        public ICommand SearchCommand { get; }

        public LocationViewModel()
        {
            _api = new GeocodingAPI();
            SearchResults = new ObservableCollection<LocationModel>();
            SearchCommand = new Command(async () => await PerformSearch());
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    _onSearchQueryChanged?.Invoke(value);
                    SearchCommand.Execute(null);
                }
            }
        }

        private async Task PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return;

            var response = await _api.GetLocationAsync(SearchQuery);
            if (response.Success)
            {
                SearchResults.Clear();
                foreach (var result in response.Data)
                {
                    SearchResults.Add(result);
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", response.ErrorMessage, "OK");
            }
        }
    }
}