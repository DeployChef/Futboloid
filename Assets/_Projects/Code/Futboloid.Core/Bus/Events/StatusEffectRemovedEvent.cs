using Futboloid.Core.StatusEffects;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct StatusEffectRemovedEvent
    {
        public int InstanceId { get; }
        public StatusEffectRemoveReason Reason { get; }

        public StatusEffectRemovedEvent(int instanceId, StatusEffectRemoveReason reason)
        {
            InstanceId = instanceId;
            Reason = reason;
        }
    }
}
