namespace WeatherApp.WeatherAPIs
{
    /// <summary>
    /// Represents a response from an API request.
    /// </summary>
    /// <typeparam name="T">The type of data contained in the response.</typeparam>
    public class APIResponse<T>
    {
        /// <summary>
        /// Indicates whether the API request was successful.
        /// </summary>
        public bool Success { get; set; }

        private string? errorMessage;
        /// <summary>
        /// Contains any error message if the API request failed.
        /// Should be set when Success is false, otherwise null.
        /// </summary>
        public string? ErrorMessage
        {
            get => Success ? null : errorMessage;
            set => errorMessage = value;
        }

        private T? data;
        /// <summary>
        /// The data returned by the API request.
        /// Should be set when Success is true, otherwise null.
        /// </summary>
        public T? Data
        {
            get => Success ? data : default;
            set => data = value;
        }
    }
}