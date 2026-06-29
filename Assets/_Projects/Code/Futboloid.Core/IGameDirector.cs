namespace Futboloid.Core
{
    public interface IGameDirector
    {
        void InitializeGame();
        void GoOnField();
        void RestartTournament();
        void ReturnToMainMenu();
        void SaveGame();
        void LoadLastSave();
    }
}
