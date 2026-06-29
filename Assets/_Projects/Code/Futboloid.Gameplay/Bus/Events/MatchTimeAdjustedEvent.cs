namespace Futboloid.Gameplay.Bus.Events
{
    /// <summary>
    /// Сдвиг оставшегося времени матча: положительный — доп. время, отрицательный — штраф.
    /// </summary>
    public readonly struct MatchTimeAdjustedEvent
    {
        public float DeltaSeconds { get; }
        public string Reason { get; }

        public MatchTimeAdjustedEvent(float deltaSeconds, string reason = null)
        {
            DeltaSeconds = deltaSeconds;
            Reason = reason;
        }
    }
}
