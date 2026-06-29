namespace Futboloid.Core.Bus.Events
{
    public readonly struct MatchTimerChangedEvent
    {
        public float RemainingSeconds { get; }
        public float Normalized { get; }

        public MatchTimerChangedEvent(float remainingSeconds, float normalized)
        {
            RemainingSeconds = remainingSeconds;
            Normalized = normalized;
        }
    }
}
