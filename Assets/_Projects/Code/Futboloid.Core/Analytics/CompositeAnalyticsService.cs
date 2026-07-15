using System.Collections.Generic;

namespace Futboloid.Core.Analytics
{
    /// <summary>Шлёт одно событие в несколько провайдеров (например Debug + UGS).</summary>
    public sealed class CompositeAnalyticsService : IAnalyticsService
    {
        private readonly IReadOnlyList<IAnalyticsService> _inner;

        public CompositeAnalyticsService(params IAnalyticsService[] inner)
        {
            _inner = inner ?? System.Array.Empty<IAnalyticsService>();
        }

        public void Track(AnalyticsEvent evt)
        {
            for (var i = 0; i < _inner.Count; i++)
                _inner[i].Track(evt);
        }

        public void SetUserProperty(string key, string value)
        {
            for (var i = 0; i < _inner.Count; i++)
                _inner[i].SetUserProperty(key, value);
        }

        public void Flush()
        {
            for (var i = 0; i < _inner.Count; i++)
                _inner[i].Flush();
        }
    }
}
