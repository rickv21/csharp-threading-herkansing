using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WeatherApp.Utils;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class SettingsPageViewModel : BindableObject
    {

        public ObservableCollection<WeatherService> WeatherServices { get; set; }

        public SettingsPageViewModel(List<WeatherService> weatherServices, Dictionary<int, List<Models.WeatherDataModel>> hourlyData, bool simulateMode)
        {
            WeatherServices = new ObservableCollection<WeatherService>(weatherServices);
            foreach (WeatherService service in weatherServices)
            {
                Debug.WriteLine(service.Name);
            }
            SimulateMode = simulateMode;
            NavigateToTestPageCommand = new Command(async () => await NavigateToTestPage());
            SaveCommand = new Command(SaveSettings);
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

        public ICommand SaveCommand { get; }
        private async void SaveSettings()
        {
            JsonFileManager jsonManager = new JsonFileManager();
            jsonManager.SetData(SimulateMode, "data", "simulateMode");

            foreach (WeatherService service in WeatherServices)
            {
                jsonManager.SetData(service.IsEnabled, "status", service.Name, "enabled");
                Debug.WriteLine(service.Name + " - " + service.IsEnabled);
            }

            await Shell.Current.GoToAsync("///Main");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand NavigateToTestPageCommand { get; }

        private async Task NavigateToTestPage() {
            await Shell.Current.GoToAsync("///TestPage");
        }

      
    }
}