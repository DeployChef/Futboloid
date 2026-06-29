namespace Futboloid.Gameplay.Bus.Events
{
    public readonly struct MatchEndedEvent
    {
        public int PlayerScore { get; }
        public int OpponentScore { get; }

        public MatchEndedEvent(int playerScore, int opponentScore)
        {
            PlayerScore = playerScore;
            OpponentScore = opponentScore;
        }
    }
}
