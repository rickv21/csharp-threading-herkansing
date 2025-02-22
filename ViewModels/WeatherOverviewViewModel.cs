﻿using Sprache;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.Utils;
using WeatherApp.Views;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Weather Overview screen. 
    /// </summary>
    public class WeatherOverviewViewModel : INotifyPropertyChanged
    {
        private readonly WeatherAppData _weatherAppData;

        // Commands for UI interaction
        public ICommand ExportCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand DayWeekCommand { get; }
        public ICommand LeftArrowCommand { get; }
        public ICommand RightArrowCommand { get; }

        private List<WeatherCondition> _dangerCons;

        private DateTime _displayedDate;
        public DateTime DisplayedDate {
            get => _displayedDate;
            set
            {
                if (_displayedDate != value)
                {
                    _displayedDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _displayedDateFormatted;

        public string DisplayedDateFormatted
        {
            get => _displayedDateFormatted;
            set
            {
                if(_displayedDateFormatted != value)
                {
                    _displayedDateFormatted = value;
                    OnPropertyChanged();
                }
            }
        }

        public Dictionary<DateTime, List<WeatherDataModel>> TimedData { get; set; } = [];

        public ObservableCollection<LocationModel> Locations { get; set; } = [];

        private LocationModel? _selectedTab;
        public LocationModel? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;

                    if (_selectedTab != null)
                    {
                        HandleTabChanged(_selectedTab);
                    }

                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<WeatherDisplayModel> _weatherItems;

        /// <summary>
        /// Collection of weather items to display in the UI.
        /// </summary>
        public ObservableCollection<WeatherDisplayModel> WeatherItems
        {
            get => _weatherItems;
            set
            {
                _weatherItems = value;
                OnPropertyChanged();
            }
        }

        private string _dayWeekButtonText;
        public string DayWeekButtonText
        {
            get => _dayWeekButtonText;
            set
            {
                if(value.Equals("Week Overzicht") || value.Equals("Dag Overzicht"))
                {
                    _dayWeekButtonText = value;
                    OnPropertyChanged();
                }
            }
        }


        private readonly Export _exporter;
		
        private bool _isPreviousButtonEnabled = true;
        public bool IsPreviousButtonEnabled
        {
            get => _isPreviousButtonEnabled;
            set
            {
                _isPreviousButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isNextButtonEnabled = true;
        public bool IsNextButtonEnabled
        {
            get => _isNextButtonEnabled;
            set
            {
                _isNextButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the ViewModel, sets default values, and starts loading data.
        /// </summary>
        /// <param name="weatherAppData">The WeatherAppData</param>
        public WeatherOverviewViewModel(WeatherAppData weatherAppData)
        {
            _weatherAppData = weatherAppData;
            _exporter = new Export();

            SetDefaultViewData();
 
            ExportCommand = new Command(Export);
            SettingsCommand = new Command(OpenSettings);
            DayWeekCommand = new Command(SwitchDayWeek);
            LeftArrowCommand = new Command(LeftArrowClick);
            RightArrowCommand = new Command(RightArrowClick);

            _dangerCons =
            [
                WeatherCondition.THUNDERSTORM,
                WeatherCondition.ICE,
                WeatherCondition.FOG,
                WeatherCondition.HAZE,
                WeatherCondition.MIST,
                WeatherCondition.TORNADO
            ];
        }

        /// <summary>
        /// Sets the default view data when the view is first loaded or reloaded.
        /// Initializes the displayed date, resets UI elements, and refreshes the location list.
        /// </summary>
        public void SetDefaultViewData()
        {
            DayWeekButtonText = "Week Overzicht";
            DisplayedDate = DateTime.Now;
            DisplayedDateFormatted = DisplayedDate.ToString("dd-MM-yyyy");

            IsPreviousButtonEnabled = false;
            IsNextButtonEnabled = true;

            //Use main thread here due to the UI not updating correctly otherwise.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Locations.Clear();
                foreach (var location in _weatherAppData.Locations)
                {
                    Locations.Add(location);
                }
            });


            WeatherItems = [];
        }

        /// <summary>
        /// Handles changes to the selected tab by updating the weather data.
        /// </summary>
        /// <param name="selectedLocation">The newly selected location.</param>
        private void HandleTabChanged(LocationModel selectedLocation)
        {
            if(WeatherItems == null)
            {
                return;
            }
            UpdateGUI();
        }


        // Event for notifying UI of property changes
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Fetches weather data asynchronously from all available weather services for a specified location and date.
        /// </summary>
        /// <param name="location">The location for which weather data is to be fetched.</param>
        /// <param name="date">The date for which weather data is required.</param>
        /// 
        /// <returns>An array of APIResponse objects containing weather data from different services.</returns>
        /// 
        /// <remarks>
        /// ### **Task Parallel Library (TPL) Overview**
        /// The TPL is a .NET framework for managing and executing parallel and asynchronous code using the **Task** abstraction.  
        /// It enables developers to perform concurrent operations efficiently by leveraging:
        /// 1. **Thread Pool Management**: Tasks are queued to the thread pool, avoiding the overhead of manually creating and destroying threads.  
        /// 2. **Concurrency Without Blocking**: Tasks allow asynchronous execution, keeping threads free for other operations.  
        /// 3. **Simplified Syntax and Error Handling**: TPL simplifies managing complex asynchronous workflows using features like continuations, `Task.WhenAll`, and `Task.WhenAny`.  
        ///
        /// #### **Why TPL is Used in This Method**
        /// In this method, TPL is used to concurrently fetch weather data from multiple weather services:
        /// - Each weather service implements an asynchronous method (`GetWeatherDataAsync`).
        /// - These methods are invoked concurrently using TPL, ensuring efficient resource utilization and reduced response time compared to sequential processing.  
        ///</remarks>
        private async Task<APIResponse<List<WeatherDataModel>>[]> FetchWeatherDataAsync()
        {
            LocationModel? location = SelectedTab;
            if(location == null)
            {
                return [];
            }

            if (_weatherAppData.WeatherServices == null || _weatherAppData.WeatherServices.Count == 0)
            {
                await Shell.Current.DisplayAlert("Error", "Er zijn geen weerservices beschikbaar!", "Oké");

                return [];
            }

            try
            {
                var usedServices = new ConcurrentBag<WeatherService>();
                IEnumerable<Task<APIResponse<List<WeatherDataModel>>>> tasks = null;

                if (DayWeekButtonText.Equals("Week Overzicht"))
                {
                    tasks = _weatherAppData.WeatherServices.Values.Where(service => service.IsEnabled)
                        .Select(async service =>
                        {
                            var response = await service.GetWeatherDataAsync(DisplayedDate, location);
                            usedServices.Add(service);
                            return response;
                        });
                }
                else
                {
                    tasks = _weatherAppData.WeatherServices.Values.Where(service => service.IsEnabled)
                        .Select(async service =>
                        {
                            var response = await service.GetWeatherForAWeekAsync(location);
                            usedServices.Add(service);
                            return response;
                        });
                }

                var results = await Task.WhenAll(tasks);
                foreach (var service in usedServices.Distinct())
                {
                    service.CountRequest();
                }

                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching weather data: {ex.Message}");
                // If we get an error here it is because an API sends a weird response.
                // During testing this happened really rarely and sending the request again had no issues.
                // So when such an error happens we just reset the locations so the tabs become unselected so another attempt can be made.
                // This is a bit of a band-aid fix but this issue is really rare and is caused by the API's.
                SetDefaultViewData();

                return [];
            }
        }

        /// <summary>
        /// Fetches and updates weather data while handling errors gracefully.
        /// The method collects data from available services and displays relevant alerts for failed requests.
        /// </summary>
        /// <returns>A dictionary containing updated weather data grouped by time.</returns>
        public async Task<Dictionary<string, WeatherDataModel>> GetUpdatedWeatherData()
        {
            //Data from all the weather services.
            var results = await FetchWeatherDataAsync();
            foreach (var result in results)
            {
                if (result.Success)
                {
                    foreach (WeatherDataModel apiData in result.Data ?? [])
                    {
                        var service = _weatherAppData.WeatherServices[result.Source];

                        DateTime periodInTime = DateTime.Now;
                        periodInTime = apiData.TimeStamp;
                        if (!TimedData.ContainsKey(periodInTime))
                        {
                            TimedData[periodInTime] = [];
                        }
                        //Add api data to a time list.
                        TimedData[periodInTime].Add(apiData);
                    }
                }
            }

            return AggregateWeatherData();
        }

        /// <summary>
        /// Aggregates weather data from multiple services and groups it by hour or day.
        /// The method processes raw weather data and extracts the relevant information for display.
        /// </summary>
        /// <returns>A dictionary where the key is either an hour or a day, and the value is the aggregated weather data.</returns>
        private Dictionary<string, WeatherDataModel> AggregateWeatherData()
        {
            var aggregatedData = TimedData.ToDictionary(entry => entry.Key, entry =>
            {
                DateTime time = entry.Key;
                var dataList = entry.Value;

                // Default values
                double totalHumidity = -1;
                double minTemperature = double.MaxValue; 
                double maxTemperature = double.MinValue;
                int validHumidityCount = 0;
                WeatherCondition aggregatedCondition = WeatherCondition.UNKNOWN;

                foreach (var data in dataList)
                {
                    // Aggregate humidity, excluding -1 values.
                    if (data.Humidity != -1)
                    {
                        totalHumidity += data.Humidity;
                        validHumidityCount++;
                    }

                    // Aggregate temperatures.
                    minTemperature = Math.Min(minTemperature, data.MinTemperature);
                    maxTemperature = Math.Max(maxTemperature, data.MaxTemperature);

                    if (aggregatedCondition == WeatherCondition.UNKNOWN)
                    {
                        aggregatedCondition = data.Condition;
                    }
                }

                double averageHumidity = validHumidityCount > 0 ? totalHumidity / validHumidityCount : 0;

                if(DayWeekButtonText.Equals("Week Overzicht"))
                {
                    return new WeatherDataModel(
                        aggregatedCondition,
                        new DateTime(DisplayedDate.Year, DisplayedDate.Month, DisplayedDate.Day, time.Hour, 0, 0),
                        minTemperature,
                        maxTemperature,
                        averageHumidity
                    );
                }
                else
                {
                    return new WeatherDataModel(
                        aggregatedCondition,
                        time,
                        minTemperature,
                        maxTemperature,
                        averageHumidity
                    );
                }
            }
         );
            Dictionary<string, WeatherDataModel> sortedAggregatedData = [];

            Func<KeyValuePair<DateTime, WeatherDataModel>, string> groupKeySelector = DayWeekButtonText.Equals("Week Overzicht")
                ? x => x.Key.Hour.ToString() // Group by hour
                : x => x.Key.DayOfWeek.ToString(); // Group by day of the week

            // Perform the grouping and sorting
            sortedAggregatedData = aggregatedData
                .OrderBy(x => x.Key)
                .GroupBy(groupKeySelector)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(x => x.Value.TimeStamp).First().Value // Take the earliest by Timestamp
                );

            var riskyData = sortedAggregatedData.Values
                .Where(item => _dangerCons.Contains(item.Condition))
                .ToList();

            DisplayAlerts(riskyData);

            return sortedAggregatedData;
        }

        /// <summary>
        /// Displays a bad weather alert for a list of weatherData
        /// </summary>
        /// <param name="data">A List of WeatherDataModels</param>
        private async void DisplayAlerts(List<WeatherDataModel> data)
        {
            if (data == null || data.Count == 0)
                return;

            // Build a message string with all alerts
            StringBuilder alertMessage = new("");

            foreach (var item in data)
            {
                if (DayWeekButtonText.Equals("Week Overzicht"))
                {
                    alertMessage.AppendLine($"Let op, om {item.TimeStamp.ToString("HH:mm")} komt er {WeatherUtils.TranslateWeatherCondition(item.Condition)} aan.");
                } 
                else
                {
                    alertMessage.AppendLine($"Let op, {WeatherUtils.TranslateDayOfTheWeek(item.TimeStamp.DayOfWeek)} komt er {WeatherUtils.TranslateWeatherCondition(item.Condition)} aan.");
                }
            }

            //Wait until weather data is visually updated.
            await Task.Delay(1000);

            // Show single alert with all messages
            App.AlertSvc.ShowAlertAsync("Slecht weer opkomst!", alertMessage.ToString(), "Ik ben gewaarschuwd");
        }

        /// <summary>
        /// Updates the UI with the latest weather data by clearing and refilling the WeatherItems collection.
        /// This ensures that the displayed weather data is always up to date.
        /// </summary>
        public async Task UpdateGUI()
        {
            if (SelectedTab == null)
            {
                return;
            }
            var data = await GetUpdatedWeatherData();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                WeatherItems.Clear();
                foreach (var entry in data)
                {
                    var model = entry.Value;
                    WeatherDisplayModel weatherItem = null;
                    // Only adds :00 if the day is displayed rather than the week.
                    if (DayWeekButtonText.Equals("Week Overzicht"))
                    {
                        string displayName = model.TimeStamp.ToString("HH:mm");
                        weatherItem = new WeatherDisplayModel(GetWeatherIcon(model.Condition), model, displayName);
                    }
                    else
                    {
             
                        string displayName = WeatherUtils.TranslateDayOfTheWeek(model.TimeStamp.DayOfWeek);

                        weatherItem = new WeatherDisplayModel(GetWeatherIcon(model.Condition), model, displayName);
                    }
                    WeatherItems.Add(weatherItem);
                }
            });
        }

        /// <summary>
        /// Retrieves the appropriate weather icon for a given weather condition.
        /// If the specified icon is not found, a fallback icon is used.
        /// </summary>
        /// <param name="condition">The weather condition to get an icon for.</param>
        /// <returns>An ImageSource representing the appropriate weather icon.</returns>
        public ImageSource GetWeatherIcon(WeatherCondition condition)
        {
            string iconName = condition.ToString().ToLower();
            string iconPath = $"WeatherApp.Resources.Images.Weather.{iconName}.png";
            var assembly = GetType().Assembly;

            try
            {
                using (var stream = assembly.GetManifestResourceStream(iconPath))
                {
                    if (stream == null)
                    {
                        return ImageSource.FromResource("WeatherApp.Resources.Images.Weather.unknown.png", assembly);
                    }

                    return ImageSource.FromResource(iconPath, assembly);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception occurred while fetching the icon: {ex.Message}. Using fallback image.");

                return ImageSource.FromResource("WeatherApp.Resources.Images.Weather.unknown.png", assembly);
            }
        }

        /// <summary>
        /// Converts weatherdata to JSON
        /// </summary>
        public string GetWeatherDataAsJson()
        {
            if (WeatherItems == null || WeatherItems.Count == 0)
            {
                return string.Empty;
            }

            return JsonSerializer.Serialize(WeatherItems, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Handles export data
        /// 
        /// Multithreading: export-method starts 3 threads to export weatherdata to JSON, CSV and TXT-files
        /// export functions are executed in parallel, potentially speeding up processing. In Export.cs
        /// 
        /// Based on paragraph 'Multithreading' in: https://stackify.com/c-threading-and-multithreading-a-guide-with-examples/
        /// </summary>
        public async void Export()
        {
            try
            {
                // Checks if there is weatherdata
                if (WeatherItems == null || WeatherItems.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Export Fout", "Geen weerdata beschikbaar om te exporteren.", "OK");
                    return;
                }

                LocationModel location = _selectedTab ?? Locations.First();

                await _exporter.ExportWeatherData(WeatherItems.ToList(), location, Locations.ToList());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting data: {ex}");
                await Shell.Current.DisplayAlert("Export Fout", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Opens the settings page.
        /// </summary>
        public async void OpenSettings()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SettingsPage());
        }

        /// <summary>
        /// Toggles between "Week Overview" and "Day Overview" modes.
        /// Updates the UI elements and reloads weather data accordingly.
        /// </summary>
        public async void SwitchDayWeek()
        {
            TimedData.Clear();

            DisplayedDate = DateTime.Now;

            if (DayWeekButtonText.Equals("Week Overzicht"))
            {
                IsPreviousButtonEnabled = false;
                IsNextButtonEnabled = false;
                DayWeekButtonText = "Dag Overzicht";
            }
            else
            {
                IsPreviousButtonEnabled = false;
                IsNextButtonEnabled = true;
                DayWeekButtonText = "Week Overzicht";
            }
            UpdateDisplayedDate();
            await UpdateGUI();
        }

        /// <summary>
        /// Updates the displayed date format based on whether the "Week Overview" or "Day Overview" mode is active.
        /// </summary>
        private void UpdateDisplayedDate()
        {
            if (DayWeekButtonText.Equals("Week Overzicht"))
            {
                DisplayedDateFormatted = DisplayedDate.ToString("dd-MM-yyyy");
            }
            else
            {
                DisplayedDateFormatted = "Komende week";
            }
        }

        /// <summary>
        /// Handles left arrow button clicks to move the displayed date back by one day.
        /// Prevents users from selecting dates in the past.
        /// </summary>
        public async void LeftArrowClick()
        {
            if(DisplayedDate.Date <= DateTime.Now)
            {
                return;
            }
            TimedData.Clear();
            DisplayedDate = DisplayedDate.AddDays(-1);
            IsPreviousButtonEnabled = DisplayedDate.Date > DateTime.Now;
            UpdateDisplayedDate();
            await UpdateGUI();
        }

        /// <summary>
        /// Handles right arrow button clicks to move the displayed date forward by one day.
        /// Updates the displayed date and reloads the weather data accordingly.
        /// </summary>
        public async void RightArrowClick()
        {
            TimedData.Clear();
            DisplayedDate = DisplayedDate.AddDays(1);
            IsPreviousButtonEnabled = true;
            UpdateDisplayedDate();
            await UpdateGUI();

            if (DisplayedDate.Date >= DateTime.Now.AddDays(13))
            {
                IsNextButtonEnabled = false;
                return;
            }
        }
    }
}
