namespace Futboloid.Core.Bus.Events
{
    public readonly struct StatusEffectRefreshedEvent
    {
        public int InstanceId { get; }
        public float DurationSeconds { get; }

        public StatusEffectRefreshedEvent(int instanceId, float durationSeconds)
        {
            InstanceId = instanceId;
            DurationSeconds = durationSeconds;
        }
    }
}
