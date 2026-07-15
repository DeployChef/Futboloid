using Cysharp.Threading.Tasks;

namespace Futboloid.Core
{
    public interface IGameDirector
    {
        NavigationState CurrentNavigation { get; }
        bool IsMatchPausedInMenu { get; }
        ITournamentBracketReadModel TournamentBracket { get; }

        UniTask InitializeGameAsync();
        void GoOnField();
        void ReturnToPause();
        void RestartTournament();
        void ReturnToMainMenu();
        void SaveGame();
        void LoadLastSave();
    }
}
