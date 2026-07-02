namespace Futboloid.Core.Bus.Events
{
    public readonly struct MatchEndedEvent
    {
        public int PlayerScore { get; }
        public int OpponentScore { get; }
        public bool PlayerWon { get; }

        public MatchEndedEvent(int playerScore, int opponentScore, bool playerWon)
        {
            PlayerScore = playerScore;
            OpponentScore = opponentScore;
            PlayerWon = playerWon;
        }
    }
}
