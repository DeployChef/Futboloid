namespace Futboloid.Core.Bus.Events
{
    public readonly struct MatchEndedEvent
    {
        public int PlayerScore { get; }
        public int OpponentScore { get; }
        public bool PlayerWon { get; }
        public MatchEndReason Reason { get; }
        public float DurationSeconds { get; }

        public MatchEndedEvent(
            int playerScore,
            int opponentScore,
            bool playerWon,
            MatchEndReason reason,
            float durationSeconds)
        {
            PlayerScore = playerScore;
            OpponentScore = opponentScore;
            PlayerWon = playerWon;
            Reason = reason;
            DurationSeconds = durationSeconds;
        }
    }
}
