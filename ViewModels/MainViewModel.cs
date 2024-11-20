using System.Diagnostics;
using System.Windows.Input;
using WeatherApp.WeatherAPIs;

namespace WeatherApp.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private int count = 0; 

        public MainViewModel()
        {
            TestCounterText = "Click to send Test API request.";
            TestAPICommand = new Command(async () => await OnTestButtonClick());
        }

        private string testCounterText;
        public string TestCounterText
        {
            get => testCounterText;
            set
            {
                testCounterText = value;
                OnPropertyChanged();
            }
        }

        public ICommand TestAPICommand { get; }

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
                var task = await api.GetWeatherDataAsync(DateTime.Today, "testlocation");
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
    }
}