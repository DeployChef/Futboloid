namespace Futboloid.Core.Bus.Events
{
    public readonly struct ComboScoreChangedEvent
    {
        public int TotalScore { get; }
        public int Multiplier { get; }
        public int DeltaPoints { get; }
        public int PreviousMultiplier { get; }

        public ComboScoreChangedEvent(int totalScore, int multiplier, int deltaPoints, int previousMultiplier)
        {
            TotalScore = totalScore;
            Multiplier = multiplier;
            DeltaPoints = deltaPoints;
            PreviousMultiplier = previousMultiplier;
        }
    }
}
