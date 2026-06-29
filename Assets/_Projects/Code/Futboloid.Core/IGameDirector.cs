namespace Futboloid.Core
{
    public interface IGameDirector
    {
        NavigationState CurrentNavigation { get; }
        bool IsMatchPausedInMenu { get; }
        ITournamentBracketReadModel TournamentBracket { get; }

        void InitializeGame();
        void GoOnField();
        void RestartTournament();
        void ReturnToMainMenu();
        void SaveGame();
        void LoadLastSave();
    }
}
