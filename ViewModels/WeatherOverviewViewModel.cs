using Sprache;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        public Dictionary<DateTime, List<WeatherDataModel>> TimedData { get; set; } = new Dictionary<DateTime, List<WeatherDataModel>>();

        public ObservableCollection<LocationModel> Locations { get; set; }


        private LocationModel _selectedTab;
        public LocationModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<WeatherDisplayItem> _weatherItems;
        /// <summary>
        /// Collection of weather items to display in the UI.
        /// </summary>
        public ObservableCollection<WeatherDisplayItem> WeatherItems
        {
            get => _weatherItems;
            set
            {
                _weatherItems = value;
                OnPropertyChanged();
            }
        }

        // Commands for UI interaction
        public ICommand ExportCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand DayWeekCommand { get; }
        public ICommand LeftArrowCommand { get; }
        public ICommand RightArrowCommand { get; }


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

        /// <summary>
        /// Initializes the ViewModel, sets default values, and starts loading data.
        /// </summary>
        public WeatherOverviewViewModel(WeatherAppData weatherAppData)
        {
            DayWeekButtonText = "Week Overzicht";
            DisplayedDate = DateTime.Now;
            WeatherItems = new ObservableCollection<WeatherDisplayItem>();
            _weatherAppData = weatherAppData;
            this.Locations = new ObservableCollection<LocationModel>();
            foreach(var location in weatherAppData.Locations)
            {
                this.Locations.Add(location);
            }

            this.SelectedTab = this.Locations.First();
            ExportCommand = new Command(Export);
            SettingsCommand = new Command(OpenSettings);
            DayWeekCommand = new Command(SwitchDayWeek);
            LeftArrowCommand = new Command(LeftArrow);
            RightArrowCommand = new Command(RightArrow);
        }

        // Event for notifying UI of property changes
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Debug.WriteLine($"Property {propertyName} changed.");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Fetches weather data asynchronously from all available weather services for a specified location and date.
        /// </summary>
        /// <param name="location">The location for which weather data is to be fetched.</param>
        /// <param name="date">The date for which weather data is required.</param>
        /// <returns>
        /// An array of <see cref="APIResponse{T}"/> objects, each containing weather data from one of the available services.
        /// Returns an empty array if no services are available or an error occurs.
        /// </returns>
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
        public async Task<APIResponse<List<WeatherDataModel>>[]> FetchWeatherDataAsync(LocationModel location, DateTime date)
        {
            Debug.WriteLine($"Fetching weather data for {location.Name} on {date}");
            if (_weatherAppData.WeatherServices == null || _weatherAppData.WeatherServices.Count == 0)
            {
                Debug.WriteLine("No services available.");
                return Array.Empty<APIResponse<List<WeatherDataModel>>>(); // Return an empty array
            }

            try
            {
                IEnumerable<Task<APIResponse<List<WeatherDataModel>>>> tasks = null;
                if (DayWeekButtonText.Equals("Week Overzicht"))
                {
                    // Fetch weather data from all services concurrently
                    tasks = _weatherAppData.WeatherServices.Values.Select(service =>
                        service.GetWeatherDataAsync(date, location, false)
                    );
                }
                else
                {
                    tasks = _weatherAppData.WeatherServices.Values.Select(service =>
                        service.GetWeatherForAWeekAsync(location, false)
                    );
                }

                Debug.WriteLine("Fetching weather data from services...");
                var results = await Task.WhenAll(tasks);
                return results; // Return results for further processing
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching weather data: {ex.Message}");
                return Array.Empty<APIResponse<List<WeatherDataModel>>>(); // Return an empty array on error
            }
        }

        /// <summary>
        /// Aggregates weather data by hour and processes it for display.
        /// </summary>
        /// <returns>Dictionary of aggregated weather data indexed by hour.</returns>
        private async Task<Dictionary<string, WeatherDataModel>> UpdateData()
        {
            LocationModel location = _weatherAppData.Locations[0]; //Temp hardcoded.
            var results = await FetchWeatherDataAsync(location, DisplayedDate);
            foreach (var result in results)
            {
                if (result.Success)
                {
                    if (result.Data.Count == 0)
                    {
                        Debug.WriteLine("Data is empty!");
                    }
                    foreach (WeatherDataModel apiData in result.Data)
                    {
                        var service = _weatherAppData.WeatherServices[apiData.APISource];
                        Debug.WriteLine(service.Name + " - " + service.IsEnabled);
                        if (!service.IsEnabled)
                        {
                            continue;
                        }
                        Debug.WriteLine(apiData.ToString());
                        DateTime periodInTime = DateTime.Now;
                        periodInTime = apiData.TimeStamp;
                        if (!TimedData.ContainsKey(periodInTime))
                        {
                            TimedData[periodInTime] = new List<WeatherDataModel>();
                        }

                        TimedData[periodInTime].Add(apiData);
                    }
                }
                else
                {
                    Debug.WriteLine(result.ErrorMessage);
                    await Shell.Current.DisplayAlert("Error", result.ErrorMessage, "OK");
                }
            } 
      
            var aggregatedData = TimedData.ToDictionary(entry => entry.Key, entry =>
            {
                DateTime time = entry.Key;
                var dataList = entry.Value;

                double totalHumidity = -1;
                double minTemperature = double.MaxValue; 
                double maxTemperature = double.MinValue;
                int validHumidityCount = 0;
                WeatherCondition aggregatedCondition = WeatherCondition.UNKNOWN; // Default value

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
                        "",
                        aggregatedCondition,
                        new DateTime(DisplayedDate.Year, DisplayedDate.Month, DisplayedDate.Day, time.Hour, 0, 0), // Set the hour.
                        minTemperature,
                        maxTemperature,
                        averageHumidity
                    );
                }
                else
                {
                    return new WeatherDataModel(
                        "",
                        aggregatedCondition,
                        time,
                        minTemperature,
                        maxTemperature,
                        averageHumidity
                    );
                }
            }
         );
            Dictionary<string, WeatherDataModel> sortedAggregatedData = new Dictionary<string, WeatherDataModel>();
            if (DayWeekButtonText.Equals("Week Overzicht"))
            {
                sortedAggregatedData = aggregatedData
                    .OrderBy(x => x.Key)
                    .GroupBy(x => "" + x.Key.Hour) // Group by the key
                    .ToDictionary(
                        group => group.Key,
                        group => group.OrderBy(x => x.Value.TimeStamp).First().Value // Take the earliest by Timestamp
                    );
            }
            else
            {
                sortedAggregatedData = aggregatedData
                    .OrderBy(x => x.Key)
                    .GroupBy(x => x.Key.DayOfWeek.ToString()) // Group by the key
                    .ToDictionary(
                        group => group.Key,
                        group => group.OrderBy(x => x.Value.TimeStamp).First().Value // Take the earliest by Timestamp
                    );
            }
            return sortedAggregatedData;
        }


        /// <summary>
        /// Updates the UI with aggregated weather data.
        /// </summary>
        public async Task UpdateGUI()
        {
            var data = await UpdateData();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                WeatherItems.Clear();
                foreach (var entry in data)
                {
                    Debug.WriteLine("Entry: " + entry);

                    var model = entry.Value;
                    WeatherDisplayItem weatherItem = null;
                    // Only adds :00 if the day is displayed rather than the week.
                    if (DayWeekButtonText.Equals("Week Overzicht"))
                    {
                        weatherItem = new WeatherDisplayItem(GetWeatherIcon(model.Condition), $"{entry.Key}:00 - {model.ToString()}", true);
                    }
                    else
                    {
                        weatherItem = new WeatherDisplayItem(GetWeatherIcon(model.Condition), $"{entry.Key} - {model.ToString()}", false);

                    }
                    Debug.WriteLine("GUI - " + model.ToString());
                    WeatherItems.Add(weatherItem);
                }
            });

            await SetupMap();
        }

        /// <summary>
        /// Fetches the appropriate weather icon for a condition.
        /// </summary>
        /// <param name="condition">The weather condition.</param>
        /// <returns>An ImageSource for the icon.</returns>
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
                        Debug.WriteLine("Icon not found, using fallback.");
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
        /// Sets up the map interface.
        /// </summary>
        public async Task SetupMap()
        {
            //TODO
        }

        /// <summary>
        /// Converts weather data to JSON
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
        /// export functions are executed in parallel, potentially speeding up processing
        /// 
        /// Based on paragraph 'Multithreading' in: https://stackify.com/c-threading-and-multithreading-a-guide-with-examples/
        /// </summary>
        public async void Export()
        {
            try
            {
                //Checks if there is weatherdata
                if (WeatherItems == null || WeatherItems.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Export Fout", "Geen weerdata beschikbaar om te exporteren.", "OK");
                    return;
                }

                string jsonData = GetWeatherDataAsJson();

                string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string exportFolder = Path.Combine(userFolder, "cshard-threading-herkansing", "ExportWeatherData");

                // Creates folder 'ExportWeatherData if it doesnt exists'
                if (!Directory.Exists(exportFolder))
                {
                    Directory.CreateDirectory(exportFolder);
                }

                // Filename based on date
                string timestamp = DateTime.Now.ToString("ddMMyyyy_HHmmss");

                // Start export threads
                await Task.WhenAll(
                    Task.Run(() => ExportToJson(jsonData, exportFolder, timestamp)),
                    Task.Run(() => ExportToCsv(exportFolder, timestamp)),
                    Task.Run(() => ExportToTxt(exportFolder, timestamp))
                );

                await Shell.Current.DisplayAlert("Export Succesvol", $"Bestanden opgeslagen in: {exportFolder}", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting data: {ex}");
                await Shell.Current.DisplayAlert("Export Fout", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Exports weather data to JSON
        /// </summary>
        private void ExportToJson(string jsonData, string folder, string timestamp)
        {
            string filePath = Path.Combine(folder, $"WeatherData_{timestamp}.json");
            File.WriteAllText(filePath, jsonData);
            Debug.WriteLine($"Weather data exported to JSON: {filePath}");
        }

        /// <summary>
        /// Exports weather data to CSV
        /// </summary>
        private void ExportToCsv(string folder, string timestamp)
        {
            string filePath = Path.Combine(folder, $"WeatherData_{timestamp}.csv");

            if (WeatherItems == null) return;

            // CSV-header 
            var csvLines = new List<string> { "Tijdstip;Weersomstandigheden;Min Temperatuur;Max Temperatuur;Vochtigheid" };

            csvLines.AddRange(WeatherItems.Select(item =>
                $"{item.TimeStamp};{item.Condition.Trim()};{GetTemperatureValue(item.MinTemp)};{GetTemperatureValue(item.MaxTemp)};{item.Humidity}"
            ));

            File.WriteAllLines(filePath, csvLines, Encoding.UTF8);
            Debug.WriteLine($"Weather data exported to CSV: {filePath}");
        }

        /// <summary>
        /// Trims min and max temperature for CSV export so as example only 8°C is displayed in the csv column
        /// </summary>
        private string GetTemperatureValue(string temperature)
        {
            var cleanedTemperature = System.Text.RegularExpressions.Regex.Replace(temperature, @"[^\d.,-]", "");
            return $"{cleanedTemperature} °C";
        }

        /// <summary>
        /// Exports weather data to TXT
        /// </summary>
        private void ExportToTxt(string folder, string timestamp)
        {
            string filePath = Path.Combine(folder, $"WeatherData_{timestamp}.txt");

            if (WeatherItems == null) return;

            var txtLines = WeatherItems.Select(item => item.DisplayText).ToList();
            File.WriteAllLines(filePath, txtLines);
            Debug.WriteLine($"Weather data exported to TXT: {filePath}");
        }


        /// <summary>
        /// Opens the settings page.
        /// </summary>
        public async void OpenSettings()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SettingsPage());
        }

        public async void SwitchDayWeek()
        {
            if(DayWeekButtonText.Equals("Week Overzicht"))
            {
                TimedData.Clear();
                DayWeekButtonText = "Dag Overzicht";
                await UpdateData();
                await UpdateGUI();
            }
            else
            {
                TimedData.Clear();
                DayWeekButtonText = "Week Overzicht";
                await UpdateData();
                await UpdateGUI();
            }
        }

        public async void LeftArrow()
        {
            if(DisplayedDate.Date <= DateTime.Now)
            {
                return;
            }
            TimedData.Clear();
            DisplayedDate = DisplayedDate.AddDays(-1);
            await UpdateData();
            await UpdateGUI();
        }
        public async void RightArrow()
        {
            TimedData.Clear();
            DisplayedDate = DisplayedDate.AddDays(1);
            await UpdateData();
            await UpdateGUI();
        }
    }
}
