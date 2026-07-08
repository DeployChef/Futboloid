namespace Futboloid.Core
{
    public interface ITournamentBracketReadModel
    {
        TournamentRunState RunState { get; }
        int CurrentMatchNumber { get; }
        int MatchesToWin { get; }
        int RunSeed { get; }
        string RoundLabel { get; }
        string StatusLine { get; }
    }
}
