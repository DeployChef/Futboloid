namespace Futboloid.Core.Leaderboards
{
    public interface IPlayerNicknameStore
    {
        string Nickname { get; }
        void Save(string nickname);
    }
}
