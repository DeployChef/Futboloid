using System.Collections.Generic;
using UnityEngine;

namespace Futboloid.Core.Pause
{
    /// <summary>
    /// Единственная точка управления <see cref="Time.timeScale"/>.
    /// Несколько подсистем могут запросить паузу независимо — снимается, когда все отпустили.
    /// </summary>
    public sealed class PauseCoordinator
    {
        private readonly HashSet<string> _reasons = new();

        public bool IsPaused => _reasons.Count > 0;

        public void Request(string reason)
        {
            if (string.IsNullOrEmpty(reason))
                return;

            if (_reasons.Add(reason))
                Apply();
        }

        public void Release(string reason)
        {
            if (string.IsNullOrEmpty(reason))
                return;

            if (_reasons.Remove(reason))
                Apply();
        }

        public void ReleaseAll()
        {
            if (_reasons.Count == 0)
                return;

            _reasons.Clear();
            Apply();
        }

        private void Apply() =>
            Time.timeScale = _reasons.Count > 0 ? 0f : 1f;
    }
}
