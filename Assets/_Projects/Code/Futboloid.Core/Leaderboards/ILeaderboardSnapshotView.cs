namespace Futboloid.Core.Leaderboards
{
    public interface ILeaderboardSnapshotView
    {
        void ApplyLoading();
        void ApplyOffline();
        void ApplyError();
        void ApplySnapshot(LeaderboardSnapshot snapshot);
    }
}
