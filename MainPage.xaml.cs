using System.Diagnostics;
using WeatherApp.WeatherAPIs;

namespace WeatherApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        TestAPI api;

        public MainPage()
        {
            InitializeComponent();
            Debug.WriteLine(AppContext.BaseDirectory);
            try
            {
                api = new TestAPI();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during initialization: {ex.Message}");
                Debug.WriteLine(ex.ToString());
            }
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            try
            {
                APIResponse<List<Models.WeatherDataModel>> task = await api.GetWeatherDataAsync(DateTime.Today, "test");
                Debug.WriteLine(task.Success);
                if (task.Success)
                {
                    foreach (Models.WeatherDataModel model in task.Data)
                    {
                        Debug.WriteLine(model.Condition);
                    }
                } else
                {
                    Debug.WriteLine(task.ErrorMessage);
                }
   
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
