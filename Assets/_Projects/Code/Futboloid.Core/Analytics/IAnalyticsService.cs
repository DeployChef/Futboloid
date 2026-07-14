namespace Futboloid.Core.Analytics
{
    public interface IAnalyticsService
    {
        void Track(AnalyticsEvent evt);

        void SetUserProperty(string key, string value);

        void Flush();
    }
}
