﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using Newtonsoft.Json;
using WeatherApp.Models;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Location screen. 
    /// </summary>
    public class LocationViewModel
    {
        private readonly GeocodingAPI _api;
        private string _searchQuery;
        private Action<string> _onSearchQueryChanged;

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

        public ObservableCollection<LocationModel> SearchResults { get; set; }
        public ICommand SearchCommand { get; }
        public ICommand SelectLocationCommand { get; }

        public LocationViewModel()
        {
            _api = new GeocodingAPI();
            SearchResults = new ObservableCollection<LocationModel>();
            SearchCommand = new Command(async () => await PerformSearch());
            SelectLocationCommand = new Command<LocationModel>(SaveLocation);
        }

        public async Task<APIResponse<List<LocationModel>>> GetLocationAsync(string query)
        {
            var response = await _api.GetLocationAsync(query);
            return response;
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

        private void SaveLocation(LocationModel location)
        {
            var locationData = new
            {
                location.Latitude,
                location.Longitude,
                location.Name
            };

            string json = JsonConvert.SerializeObject(locationData, Formatting.Indented);
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "selected_location.json");
            File.WriteAllText(filePath, json);

            Application.Current.MainPage.DisplayAlert("Saved", "Location saved successfully!", "OK");
        }
    }
}