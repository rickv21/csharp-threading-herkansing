using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.Utils;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class LocationViewModel
    {
        private readonly GeocodingAPI _api;
        private readonly OpenWeatherMapAPI _weatherAPI;
        private string _searchQuery;
        private Action<string> _onSearchQueryChanged;
        private CancellationTokenSource _debounceCts;

        public ObservableCollection<LocationModel> SearchResults { get; set; }
        public ObservableCollection<LocationModel> SavedLocations { get; set; } = new ObservableCollection<LocationModel>();

        public ICommand SearchCommand { get; }

        public ICommand RemoveLocationCommand { get; }

        public LocationViewModel()
        {
            _api = new GeocodingAPI();
            _weatherAPI = new OpenWeatherMapAPI();
            RemoveLocationCommand = new Command<LocationModel>(async (location) => await RemoveLocationAsync(location));
            SavedLocations = new ObservableCollection<LocationModel>();
            SearchResults = new ObservableCollection<LocationModel>();
            SearchCommand = new Command(async () => await PerformSearch());
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

                    JObject locationsObject = new JObject();

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
            // Clear existing data and add new saved locations
            SavedLocations.Clear();
            foreach (var location in savedLocations)
            {
                // Update the current weather data for each location
                GetWeatherForSavedLocationAsync(location);
                SavedLocations.Add(location);
                // Update the JSON file with the new weather data
                UpdatePlacesJson();
            }
        }

        /// <summary>
        /// Load the locations from the places.json
        /// </summary>
        /// <param name="filePath">The file path of places.json</param>
        /// <returns>A list of favorited locations</returns>
        private static List<LocationModel> LoadLocationsFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        var jsonObject = JObject.Parse(json);
                        var locationsObject = jsonObject["Locations"] as JObject;

                        if (locationsObject != null)
                        {
                            var locationsArray = locationsObject.Properties()
                                                                .Select(prop => prop.Value)
                                                                .ToArray();

                            var locations = locationsArray.Select(value => value.ToObject<LocationModel>())
                                                          .Where(location => location != null)
                                                          .ToList();

                            return locations;
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
            List<LocationModel> array = LoadLocationsFromFile(GetFilePath());

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
        public async Task<SaveLocationResult> SaveSelectedLocation(LocationModel selectedLocation)
        {
            try
            {
                if (HasReachedFavoriteLimit())
                {
                    return SaveLocationResult.FavoriteLimitReached;
                }

                JsonFileManager jsonFileManager = new JsonFileManager(GetFilePath());
                var root = jsonFileManager.GetAllJson();
                var locationsToken = root["Locations"] as JObject ?? new JObject();
                string placeId = selectedLocation.PlaceId;

                // Check if the location already exists
                if (IsLocationDuplicate(locationsToken, selectedLocation))
                {
                    return SaveLocationResult.DuplicateLocation;
                }

                // Fetch the weather data for the location before saving
                GetWeatherForSavedLocationAsync(selectedLocation);

                var locationObject = new JObject
                {
                    ["Name"] = selectedLocation.Name,
                    ["Latitude"] = selectedLocation.Latitude,
                    ["Longitude"] = selectedLocation.Longitude,
                    ["Country"] = selectedLocation.Country,
                    ["State"] = selectedLocation.State,
                    ["WeatherData"] = JArray.FromObject(selectedLocation.WeatherData)
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

        /// <summary>
        /// Threadpool
        /// ==========
        /// Threadpool is used here to retrieve the weather data for all the favorited locations
        /// Each location has it's own UserWorkitem, which then get executed 1 by 1
        /// When a task is finished, a signal is sent so the next task can get started
        /// At the end, it waits for all tasks to finish before exiting the method
        /// ====================================================================================
        /// Retrieve the weatherdata of a location
        /// </summary>
        /// <param name="location">The specified location</param>
        /// <returns>A completed <see cref="Task"/></returns>
        public void GetWeatherForSavedLocationAsync(LocationModel location)
        {
            // Create a CountdownEvent to signal when all tasks are completed
            var countdown = new CountdownEvent(SavedLocations.Count());

            try
            {
                // Queue a work item to the ThreadPool
                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    try
                    {
                        // Fetch weather data for the location
                        await FetchWeatherForLocationAsync(location);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error fetching weather for {location.Name}: {ex.Message}");
                    }
                    finally
                    {
                        // Signal when the work is done
                        countdown.Signal();
                    }
                });

                // Wait for all tasks to complete
                countdown.Wait();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching weather for saved locations: {ex.Message}");
            }
        }

        private async Task FetchWeatherForLocationAsync(LocationModel location)
        {
            try
            {
                var response = await _weatherAPI.GetCurrentWeatherAsync(location);

                if (response.Success)
                {
                    location.WeatherData = [response.Data];
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
    }
}
