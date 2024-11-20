using System.Diagnostics;

namespace WeatherApp.WeatherAPIs
{
    public class ServiceRequestLimit(int requestLimit, int currentRequestCount)
    {
        public int CurrentRequestCount { get; set; } = currentRequestCount;
        public int RequestLimit { get; private set; } = requestLimit;

        public void CountRequest()
        {
            CurrentRequestCount++;
            Debug.WriteLine(CurrentRequestCount);
        }

        public bool HasReachedLimit()
        {
            if(RequestLimit <= 0) return false;
            return CurrentRequestCount >= RequestLimit;
        }
    }
}
