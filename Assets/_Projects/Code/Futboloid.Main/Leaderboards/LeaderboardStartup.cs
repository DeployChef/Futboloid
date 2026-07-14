using Cysharp.Threading.Tasks;
using Futboloid.Core.Leaderboards;
using VContainer.Unity;

namespace Futboloid.Main.Leaderboards
{
    public sealed class LeaderboardStartup : IStartable
    {
        private readonly ILeaderboardService _leaderboard;

        public LeaderboardStartup(ILeaderboardService leaderboard)
        {
            _leaderboard = leaderboard;
        }

        public void Start()
        {
            _leaderboard.InitializeAsync().Forget();
        }
    }
}
