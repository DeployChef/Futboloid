namespace Futboloid.Gameplay.Bus.Events
{
    public readonly struct MatchScoreChangedEvent
    {
        public int PlayerScore { get; }
        public int OpponentScore { get; }

        public MatchScoreChangedEvent(int playerScore, int opponentScore)
        {
            PlayerScore = playerScore;
            OpponentScore = opponentScore;
        }
    }
}
