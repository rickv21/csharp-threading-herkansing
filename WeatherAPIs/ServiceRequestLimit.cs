using System.Diagnostics;

namespace WeatherApp.WeatherAPIs
{
    /// <summary>
    /// Manages the request limit for a service, tracking the number of requests made.
    /// </summary>
    public class ServiceRequestLimit(int requestLimit, int currentRequestCount)
    {
        /// <summary>
        /// The current number of requests made.
        /// </summary>
        public int CurrentRequestCount { get; set; } = currentRequestCount;

        /// <summary>
        /// The maximum number of requests allowed.
        /// </summary>
        public int RequestLimit { get; private set; } = requestLimit;

        /// <summary>
        /// Increments the request count.
        /// </summary>
        public void CountRequest()
        {
            CurrentRequestCount++;
            Debug.WriteLine(CurrentRequestCount);
        }

        /// <summary>
        /// Determines if the request limit has been reached.
        /// </summary>
        /// <returns>True if the current request count is greater than or equal to the request limit; otherwise, false.</returns>
        public bool HasReachedLimit()
        {
            if (RequestLimit <= 0) return false;
            return CurrentRequestCount >= RequestLimit;
        }
    }
}
