namespace WeatherApp.Alerts
{
    public class AlertService : IAlertService
    {

        /// <summary>
        /// Show a alert with the given variables.
        /// This should be run async.
        /// </summary>
        public Task ShowAlert(string title, string message, string cancel = "OK")
        {
            return Application.Current!.MainPage!.DisplayAlert(title, message, cancel);
        }

        /// <summary>
        /// Show a confirmation alert with the given variables.
        /// </summary>
        public Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
        {
            return Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel);
        }

        /// <summary>
        /// Show a alert with the given variables.
        /// </summary>
        public void ShowAlertAsync(string title, string message, string cancel = "OK")
        {
            Application.Current!.MainPage!.Dispatcher.Dispatch(async () =>
                await ShowAlert(title, message, cancel)
            );
        }

        /// <summary>
        /// Show a confimation alert with the given variables.
        /// </summary>
        public void ShowConfirmation(string title, string message, Action<bool> callback,
                                     string accept = "Yes", string cancel = "No")
        {
            Application.Current!.MainPage!.Dispatcher.Dispatch(async () =>
            {
                bool answer = await ShowConfirmationAsync(title, message, accept, cancel);
                callback(answer);
            });
        }
    }
}