using UnityEngine;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct StatusEffectAppliedEvent
    {
        public int InstanceId { get; }
        public string EffectId { get; }
        public string Title { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public bool IsDebuff { get; }
        public float DurationSeconds { get; }
        public int Charges { get; }

        public StatusEffectAppliedEvent(
            int instanceId,
            string effectId,
            string title,
            string description,
            Sprite icon,
            bool isDebuff,
            float durationSeconds,
            int charges)
        {
            InstanceId = instanceId;
            EffectId = effectId;
            Title = title;
            Description = description;
            Icon = icon;
            IsDebuff = isDebuff;
            DurationSeconds = durationSeconds;
            Charges = charges;
        }
    }
}
