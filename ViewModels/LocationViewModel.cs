using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.Utils;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class LocationViewModel : INotifyPropertyChanged
    {
        private readonly GeocodingAPI _api;
        private readonly OpenWeatherMapAPI _weatherAPI;
        private string _searchQuery;
        private Action<string> _onSearchQueryChanged;
        private CancellationTokenSource _debounceCts;

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

        public LocationViewModel()
        {
            _api = new GeocodingAPI();
            _weatherAPI = new OpenWeatherMapAPI();
            RemoveLocationCommand = new Command<LocationModel>(async (location) => await RemoveLocationAsync(location));
            SavedLocations = [];
            SearchResults = [];
            SearchCommand = new Command(async () => await PerformSearch());
            FetchWeatherDataCommand = new Command(FetchWeatherDataForAllLocations);
            LoadSavedLocations();
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
                UpdatePlacesJson();
            }
        }

        /// <summary>
        /// Update the JSON file with favorite places
        /// </summary>
        private void UpdatePlacesJson()
        {
            try
            {
                string filePath = GetFilePath();

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    JObject jsonObject = JObject.Parse(json);
                    JArray geocodingArray = (JArray)jsonObject["Geocoding"] ?? [];

                    JObject locationsObject = [];

                    foreach (var location in SavedLocations)
                    {
                        string placeId = location.PlaceId ?? Guid.NewGuid().ToString(); // Ensure a unique place_id
                        locationsObject[placeId] = JObject.FromObject(new
                        {
                            location.Name,
                            location.Latitude,
                            location.Longitude,
                            location.Country,
                            location.State,
                            WeatherData = location.WeatherData != null ? JArray.FromObject(location.WeatherData) : []
                        });
                    }

                    JObject updatedJson = new()
                    {
                        ["Geocoding"] = geocodingArray,
                        ["Locations"] = locationsObject
                    };

                    File.WriteAllText(filePath, updatedJson.ToString(Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the favorite places JSON file", ex);
            }
        }

        /// <summary>
        /// Get the file path of the places.json file
        /// </summary>
        /// <returns>The file path</returns>
        private string GetFilePath()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "places.json");

            return filePath;
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
        /// Load the saved locations from the places.json
        /// </summary>
        public void LoadSavedLocations()
        {
            var savedLocations = LoadLocationsFromFile(GetFilePath());
            SavedLocations.Clear();
            foreach (var location in savedLocations)
            {
                SavedLocations.Add(location);
            }
        }

        /// <summary>
        /// Load the locations from the places.json
        /// </summary>
        /// <param name="filePath">The file path of places.json</param>
        /// <returns>An observable collection of favorited locations</returns>
        private static ObservableCollection<LocationModel> LoadLocationsFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        var jsonObject = JObject.Parse(json);

                        if (jsonObject["Locations"] is JObject locationsObject)
                        {
                            var locationsArray = locationsObject.Properties()
                                                                .Select(prop => prop.Value)
                                                                .ToArray();

                            var locations = locationsArray.Select(value => value.ToObject<LocationModel>())
                                                          .Where(location => location != null)
                                                          .ToList();

                            return new ObservableCollection<LocationModel>(locations);
                        }

                        return [];
                    }
                    catch (JsonException ex)
                    {
                        return [];
                    }
                }
            }

            return [];
        }

        /// <summary>
        /// Check if a location has already been added as a favorite
        /// </summary>
        /// <param name="locationsToken">The locations token containing all saved locations</param>
        /// <param name="selectedLocation">The location to be added</param>
        /// <returns></returns>
        private bool IsLocationDuplicate(JObject locationsToken, LocationModel selectedLocation)
        {
            return locationsToken.Properties().Any(prop =>
            {
                var loc = prop.Value as JObject;
                var latitude = loc?["Latitude"]?.Value<double>();
                var longitude = loc?["Longitude"]?.Value<double>();

                return latitude == selectedLocation.Latitude && longitude == selectedLocation.Longitude;
            });
        }

        /// <summary>
        /// Check if the favorites limit has been reached
        /// </summary>
        /// <returns>True if reached, false if not reached</returns>
        public bool HasReachedFavoriteLimit()
        {
            ObservableCollection<LocationModel> array = LoadLocationsFromFile(GetFilePath());

            if (array.Count == 5)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Save the selected location to places.json
        /// </summary>
        /// <param name="selectedLocation">The selected location</param>
        /// <returns>True if saved, false if the location already exists</returns>
        public SaveLocationResult SaveSelectedLocation(LocationModel selectedLocation)
        {
            try
            {
                if (HasReachedFavoriteLimit())
                {
                    return SaveLocationResult.FavoriteLimitReached;
                }

                JsonFileManager jsonFileManager = new(GetFilePath());
                var root = jsonFileManager.GetAllJson();
                var locationsToken = root["Locations"] as JObject ?? [];
                string placeId = selectedLocation.PlaceId;

                // Check if the location already exists
                if (IsLocationDuplicate(locationsToken, selectedLocation))
                {
                    return SaveLocationResult.DuplicateLocation;
                }

                var locationObject = new JObject
                {
                    ["Name"] = selectedLocation.Name,
                    ["Latitude"] = selectedLocation.Latitude,
                    ["Longitude"] = selectedLocation.Longitude,
                    ["Country"] = selectedLocation.Country,
                    ["State"] = selectedLocation.State,
                    ["WeatherData"] = null
                };

                // Add the location with place_id as the key
                locationsToken[placeId] = locationObject;
                root["Locations"] = locationsToken;
                jsonFileManager.SaveAllJson(root);
                SavedLocations.Add(selectedLocation);

                return SaveLocationResult.Success;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the selected location", ex);
            }
        }

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
                    Debug.WriteLine($"Weather data for {location.Name}: {string.Join(", ", location.WeatherData.Select(data => data.ToString()))}");
                }
                else
                {
                    location.WeatherData = [];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching weather for {location.Name}: {ex.Message}");
                location.WeatherData = [];
            }
        }

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
            UpdatePlacesJson();
        }
    }
}
