using UnityEngine;

namespace Futboloid.Core.StatusEffects
{
    public readonly struct ActiveEffectSnapshot
    {
        public string EffectId { get; }
        public Sprite Icon { get; }
        public float Duration01 { get; }
        public float RemainingSeconds { get; }
        public int Charges { get; }
        public bool ShowTimerRing { get; }

        public ActiveEffectSnapshot(
            string effectId,
            Sprite icon,
            float duration01,
            float remainingSeconds,
            int charges,
            bool showTimerRing)
        {
            EffectId = effectId;
            Icon = icon;
            Duration01 = duration01;
            RemainingSeconds = remainingSeconds;
            Charges = charges;
            ShowTimerRing = showTimerRing;
        }
    }
}
