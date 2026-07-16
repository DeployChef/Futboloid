using System.Text;
using UnityEngine;

namespace Futboloid.Core.Analytics
{
    public sealed class DebugAnalyticsService : IAnalyticsService
    {
        public void Track(AnalyticsEvent evt)
        {
            if (string.IsNullOrEmpty(evt.Name))
                return;

            Debug.Log($"[Analytics] {Format(evt)}");
        }

        public void SetUserProperty(string key, string value) =>
            Debug.Log($"[Analytics] user.{key} = {value}");

        public void Flush() =>
            Debug.Log("[Analytics] Flush");

        private static string Format(AnalyticsEvent evt)
        {
            if (evt.Parameters == null || evt.Parameters.Count == 0)
                return evt.Name;

            var sb = new StringBuilder(evt.Name);
            sb.Append(' ');
            var first = true;
            foreach (var pair in evt.Parameters)
            {
                if (!first)
                    sb.Append(", ");
                first = false;
                sb.Append(pair.Key).Append('=').Append(pair.Value);
            }

            return sb.ToString();
        }
    }
}
