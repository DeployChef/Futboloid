namespace Futboloid.Core.Leaderboards
{
    public readonly struct LeaderboardEntryDto
    {
        public long Rank { get; }
        public string PlayerName { get; }
        public int Score { get; }

        public LeaderboardEntryDto(long rank, string playerName, int score)
        {
            Rank = rank;
            PlayerName = playerName ?? string.Empty;
            Score = score;
        }
    }
}
