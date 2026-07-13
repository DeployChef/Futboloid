using Cysharp.Threading.Tasks;

namespace Futboloid.Core.Leaderboards
{
    public interface ILeaderboardService
    {
        bool IsInitialized { get; }
        bool IsSignedIn { get; }

        UniTask InitializeAsync();
        UniTask SetNicknameAsync(string nickname);
        UniTask SubmitRunScoreAsync(int score);
        UniTask<LeaderboardSnapshot> FetchSnapshotAsync(int topCount = 10);
    }
}
