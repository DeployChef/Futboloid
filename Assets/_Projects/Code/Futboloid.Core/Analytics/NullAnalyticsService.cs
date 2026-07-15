namespace Futboloid.Core.Analytics
{
    public sealed class NullAnalyticsService : IAnalyticsService
    {
        public void Track(AnalyticsEvent evt)
        {
        }

        public void SetUserProperty(string key, string value)
        {
        }

        public void Flush()
        {
        }
    }
}
