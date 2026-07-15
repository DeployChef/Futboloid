using System.Collections.Generic;

namespace Futboloid.Core.Analytics
{
    public readonly struct AnalyticsEvent
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }

        public AnalyticsEvent(string name, IReadOnlyDictionary<string, object> parameters = null)
        {
            Name = name;
            Parameters = parameters;
        }
    }
}
