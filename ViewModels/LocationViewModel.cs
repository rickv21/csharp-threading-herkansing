﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.Utils;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class LocationViewModel : INotifyPropertyChanged
    {
        private readonly WeatherAppData _weatherAppData;
        private readonly GeocodingAPI _api;
        private readonly OpenWeatherMapAPI _weatherAPI;
        private string _searchQuery;
        private Action<string> _onSearchQueryChanged;
        private CancellationTokenSource _debounceCts;
        private PlacesManager _placesManager;

        public ObservableCollection<LocationModel> SearchResults { get; set; }

        private ObservableCollection<LocationModel> _savedLocations;

        public ObservableCollection<LocationModel> SavedLocations
        {
            get { return _savedLocations; }
            set
            {
                if (_savedLocations != value)
                {
                    _savedLocations = value;
                    OnPropertyChanged(nameof(SavedLocations));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand SearchCommand { get; }
        public ICommand RemoveLocationCommand { get; }
        public ICommand FetchWeatherDataCommand { get; }

        public LocationViewModel(WeatherAppData weatherAppData)
        {
            _weatherAppData = weatherAppData;
            _placesManager = new PlacesManager();
            _api = new GeocodingAPI();
            _weatherAPI = new OpenWeatherMapAPI();
            RemoveLocationCommand = new Command<LocationModel>(async (location) => await RemoveLocationAsync(location));
            SavedLocations = [];
            SearchResults = [];
            SearchCommand = new Command(async () => await PerformSearch());
            FetchWeatherDataCommand = new Command(FetchWeatherDataForAllLocations);

            LoadSavedLocations();
        }

        private void LoadSavedLocations()
        {
            SavedLocations.Clear();
            foreach (var location in _weatherAppData.Locations)
            {
                SavedLocations.Add(location);
            }
        }

        public SaveLocationResult SaveSelectedLocation(LocationModel selectedLocation)
        {
            if (HasReachedFavoriteLimit())
            {
                return SaveLocationResult.FavoriteLimitReached;
            }

            SaveLocationResult result = _placesManager.SaveLocation(selectedLocation);
            if(result == SaveLocationResult.Success)
            {
                SavedLocations.Add(selectedLocation);
            }
            return result;
        }

        /// <summary>
        /// Check if the favorites limit has been reached
        /// </summary>
        /// <returns>True if reached, false if not reached</returns>
        public bool HasReachedFavoriteLimit()
        {
            return SavedLocations.Count >= 5;
        }

        /// <summary>
        /// The searchQuery an user enters
        /// </summary>
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

        /// <summary>
        /// Remove a location from favorites
        /// </summary>
        /// <param name="location">The location to be removed</param>
        private async Task RemoveLocationAsync(LocationModel location)
        {
            if (location == null)
                return;

            bool isConfirmed = await Application.Current.MainPage.DisplayAlert("Bevestig verwijdering",
                $"Weet je zeker dat je de locatie: {location.Name} wilt verwijderen?",
                "Bevestigen", "Annuleren");

            if (isConfirmed)
            {
                SavedLocations.Remove(location);
                _placesManager.UpdatePlacesJson(SavedLocations);
            }
        }

        /// <summary>
        /// Perform a search request on the Geocoding API
        /// </summary>
        /// <returns>A task representing the async operation</returns>
        private async Task PerformSearch()
        {
            if (_debounceCts != null)
            {
                _debounceCts.Cancel();
                _debounceCts.Dispose();
            }

            _debounceCts = new CancellationTokenSource();
            try
            {
                await Task.Delay(1500, _debounceCts.Token);
                if (string.IsNullOrWhiteSpace(SearchQuery)) return;

                if (SearchQuery.Length > 2)
                {
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
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled due to a new keystroke
            }
        }

        /// <summary>
        /// Fetch the weather data for a location
        /// </summary>
        /// <param name="location">The specified location</param>
        /// <returns>A task</returns>
        private async Task FetchWeatherForLocationAsync(LocationModel location)
        {
            try
            {
                WeatherDataModel lastWeatherData = location.WeatherData?.FirstOrDefault();
                if (lastWeatherData != null)
                {
                    DateTime timestamp = lastWeatherData.TimeStamp;
                    // Check if the data is less than an hour old
                    if ((DateTime.Now - timestamp).TotalHours < 1)
                    {
                        Debug.WriteLine($"Using cached weather data for {location.Name} (retrieved at {timestamp})");
                        return; // Skip fetching data from the API
                    }
                }

                var response = await _weatherAPI.GetCurrentWeatherAsync(location);

                if (response.Success)
                {
                    location.WeatherData = new ObservableCollection<WeatherDataModel> { response.Data };
                    location.IsWeatherDataAvailable = true;
                    Debug.WriteLine($"Weather data for {location.Name}: {string.Join(", ", location.WeatherData.Select(data => data.ToString()))}");
                }
                else
                {
                    location.WeatherData = new ObservableCollection<WeatherDataModel>();
                    location.IsWeatherDataAvailable = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching weather for {location.Name}: {ex.Message}");
                location.WeatherData = new ObservableCollection<WeatherDataModel>();
            }
        }

        /// <summary>
        /// THREADPOOL
        /// ==========
        /// In this method, a ThreadPool is used to gather weatherdata for all of the favorited locations
        /// It loops through the saved locations and starts a workitem for each location
        /// When an item has been looped and has been executed successfully, a signal is sent by using the countdown to let the method know the next task can be executed
        /// ==========
        /// Gather all weather data for the saved locations
        /// </summary>
        private void FetchWeatherDataForAllLocations()
        {
            var countdown = new CountdownEvent(SavedLocations.Count);

            foreach (var location in SavedLocations)
            {
                // Queue work to the ThreadPool
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        var fetchTask = FetchWeatherForLocationAsync(location);

                        fetchTask.ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                Debug.WriteLine($"Error fetching weather for {location.Name}: {t.Exception?.Message}");
                            }

                            countdown.Signal();
                        }, TaskContinuationOptions.ExecuteSynchronously);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching weather for {location.Name}: {ex.Message}");
                        countdown.Signal();
                    }
                });
            }

            countdown.Wait();
            _placesManager.UpdatePlacesJson(SavedLocations);
        }
    }
}
