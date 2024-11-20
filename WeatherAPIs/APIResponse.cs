namespace WeatherApp.WeatherAPIs
{
    public class APIResponse<T> {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; } }
}
