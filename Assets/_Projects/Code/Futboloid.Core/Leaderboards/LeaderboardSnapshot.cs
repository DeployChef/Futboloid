using System.Collections.Generic;

namespace Futboloid.Core.Leaderboards
{
    public sealed class LeaderboardSnapshot
    {
        public LeaderboardStatus Status { get; set; } = LeaderboardStatus.Loading;
        public IReadOnlyList<LeaderboardEntryDto> TopEntries { get; set; } = new List<LeaderboardEntryDto>();
        public bool HasPlayerEntry { get; set; }
        public LeaderboardEntryDto PlayerEntry { get; set; }
        public int TotalPlayers { get; set; }
    }
}
