﻿namespace WeatherApp.Alerts
{
    public interface IAlertService
    {
        Task ShowAlert(string title, string message, string cancel = "OK");
        Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No");

        void ShowAlertAsync(string title, string message, string cancel = "OK");
        void ShowConfirmation(string title, string message, Action<bool> callback,
                              string accept = "Yes", string cancel = "No");
    }
}