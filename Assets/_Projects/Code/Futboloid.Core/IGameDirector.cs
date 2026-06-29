namespace Futboloid.Core
{
    public interface IGameDirector
    {
        NavigationState CurrentNavigation { get; }
        bool IsMatchPausedInMenu { get; }

        void InitializeGame();
        void GoOnField();
        void RestartTournament();
        void ReturnToMainMenu();
        void SaveGame();
        void LoadLastSave();
    }
}
