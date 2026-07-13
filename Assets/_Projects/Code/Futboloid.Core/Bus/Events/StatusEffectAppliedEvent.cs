using UnityEngine;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct StatusEffectAppliedEvent
    {
        public int InstanceId { get; }
        public string EffectId { get; }
        public Sprite Icon { get; }
        public bool IsDebuff { get; }
        public float DurationSeconds { get; }
        public int Charges { get; }

        public StatusEffectAppliedEvent(
            int instanceId,
            string effectId,
            Sprite icon,
            bool isDebuff,
            float durationSeconds,
            int charges)
        {
            InstanceId = instanceId;
            EffectId = effectId;
            Icon = icon;
            IsDebuff = isDebuff;
            DurationSeconds = durationSeconds;
            Charges = charges;
        }
    }
}
