namespace Futboloid.Core
{
    /// <summary>
    /// Прогресс турнира в App scope: запись результатов и сброс забега.
    /// </summary>
    public interface ITournamentRunService : ITournamentBracketReadModel
    {
        void ResetRun();
        void RecordMatchResult(int playerScore, int opponentScore, bool playerWon);
    }
}
