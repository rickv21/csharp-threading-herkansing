using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private int count = 0;
        public ICommand OpenWeerLiveCommand { get; }


        public MainViewModel()
        {
            TestCounterText = "Click to send Test API request.";
            TestAPICommand = new Command(async () => await OnTestButtonClick());
            OpenWeatherMapCommand = new Command(async () => await OnOpenWeatherMapClick());
            OpenWeerLiveCommand = new Command(ExecuteOpenWeerLiveCommand);
            IsOpenWeatherMapDay = true;
            SimulateMode = false;
        }

        private string _testCounterText;
        public string TestCounterText
        {
            get => _testCounterText;
            set
            {
                _testCounterText = value;
                OnPropertyChanged();
            }
        }

        private bool _simulateMode;
        public bool SimulateMode
        {
            get => _simulateMode;
            set
            {
                _simulateMode = value;
                OnPropertyChanged();
            }
        }

        private bool _openWeatherMapDay;
        private bool _openWeatherMapWeek;

        public bool IsOpenWeatherMapDay
        {
            get => _openWeatherMapDay;
            set
            {
                if (_openWeatherMapDay != value)
                {
                    _openWeatherMapDay = value;
                    if (value) IsOpenWeatherMapWeek = false; // Ensure only one option is selected
                    OnPropertyChanged();
                }
            }
        }

        public bool IsOpenWeatherMapWeek
        {
            get => _openWeatherMapWeek;
            set
            {
                if (_openWeatherMapWeek != value)
                {
                    _openWeatherMapWeek = value;
                    if (value) _openWeatherMapDay = false; // Ensure only one option is selected
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand TestAPICommand { get; }

        public ICommand OpenWeatherMapCommand { get; }

        private async Task OnTestButtonClick()
        {
            TestAPI api;
            try
            {
                api = new TestAPI();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading Test API: {ex.Message}");
                return;
            }

            count++;
            TestCounterText = count == 1 ? $"Clicked {count} time" : $"Clicked {count} times";

            try
            {
                var task = await api.GetWeatherDataAsync(DateTime.Today, "testlocation", SimulateMode);
                Debug.WriteLine("Is success: " + task.Success);
                if (task.Success)
                {
                    //An assertion to throw a exception if Data is null when Success is true, which should never happen.
                    Debug.Assert(task.Data != null, "task.Data should not be null when task.Success is true");

                    //It is supposed to return a list for each hour, but for this test API we only add 1 WeatherDataModel to the list.
                    foreach (var model in task.Data)
                    {
                        Debug.WriteLine(model.ToString());

                        // Show a simple alert
                        await Shell.Current.DisplayAlert("Weather Condition", model.ToString(), "OK");
                    }
                }
                else
                {
                    Debug.WriteLine(task.ErrorMessage);
                    await Shell.Current.DisplayAlert("Error", task.ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
            }
        }

        private async Task OnOpenWeatherMapClick()
        {
            OpenWeatherMapAPI api;
            try
            {
                api = new OpenWeatherMapAPI();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading Open Weather Map API: {ex.Message}");
                return;
            }

            try
            {
                APIResponse<List<WeatherDataModel>> task;
                if (IsOpenWeatherMapDay)
                {
                    task = await api.GetWeatherDataAsync(DateTime.Today, "Emmen", SimulateMode);
                } 
                else
                {
                    task = await api.GetWeatherForAWeekAsync("Emmen", SimulateMode);
                }
            
                Debug.WriteLine("Is success: " + task.Success);
                if (task.Success)
                {
                    Debug.WriteLine(task.Data);
                    //An assertion to throw a exception if Data is null when Success is true, which should never happen.
                    Debug.Assert(task.Data != null, "task.Data should not be null when task.Success is true");

                    //OpenWeatherMap only supports showing weather for every 3 hours -.-
                    foreach (var model in task.Data)
                    {
                        Debug.WriteLine("Model loop!");
                        Debug.WriteLine(model.ToString());

                        // Show a simple alert
                        await Shell.Current.DisplayAlert("Weather Condition", model.ToString(), "OK");
                    }
                }
                else
                {
                    Debug.WriteLine(task.ErrorMessage);
                    await Shell.Current.DisplayAlert("Error", task.ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
            }
        }
        private async void ExecuteOpenWeerLiveCommand()
        {
            WeerLiveAPI api;
            try
            {
                api = new WeerLiveAPI();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading WeerLive API: {ex.Message}");
                return;
            }

            try
            {
                APIResponse<List<WeatherDataModel>> task;
                task = await api.GetWeatherDataAsync(DateTime.Today, "Emmen", SimulateMode);
               
                Debug.WriteLine("Is success: " + task.Success);
                if (task.Success)
                {
                    Debug.WriteLine(task.Data);
                    //An assertion to throw a exception if Data is null when Success is true, which should never happen.
                    Debug.Assert(task.Data != null, "task.Data should not be null when task.Success is true");

                }
                else
                {
                    Debug.WriteLine(task.ErrorMessage);
                    await Shell.Current.DisplayAlert("Error", task.ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
            }
        }
    }
}