namespace WeatherApp.Utils
{
    /// <summary>
    /// The possible results of saving a location as a favorite
    /// </summary>
    public enum SaveLocationResult
    {
        Success,
        DuplicateLocation,
        FavoriteLimitReached
    }
}