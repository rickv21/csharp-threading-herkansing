public interface IAlertService
{
    // ----- async calls (MUST BE ON DISPATCHER THREAD) -----
    Task ShowAlertAsync(string title, string message, string cancel = "OK");
    Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No");

    void ShowAlert(string title, string message, string cancel = "OK");
    /// <param name="callback">Action to perform afterwards.</param>
    void ShowConfirmation(string title, string message, Action<bool> callback,
                          string accept = "Yes", string cancel = "No");
}

internal class AlertService : IAlertService
{

    public Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        return Application.Current!.MainPage!.DisplayAlert(title, message, cancel);
    }

    public Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        return Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel);
    }


    // ----- "Fire and forget" calls -----

    /// <summary>
    /// "Fire and forget". Method returns BEFORE showing alert.
    /// </summary>
    public void ShowAlert(string title, string message, string cancel = "OK")
    {
        Application.Current!.MainPage!.Dispatcher.Dispatch(async () =>
            await ShowAlertAsync(title, message, cancel)
        );
    }

    /// <summary>
    /// "Fire and forget". Method returns BEFORE showing alert.
    /// </summary>
    /// <param name="callback">Action to perform afterwards.</param>
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