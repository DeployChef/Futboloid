namespace Futboloid.Core
{
    public interface IGameDirector
    {
        void InitializeGame();
        void RestartTournament();
        void RestartMatch();
        void ReturnToMainMenu();
        void SaveGame();
        void LoadLastSave();
    }
}
