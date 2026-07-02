namespace Futboloid.Core
{
    public interface ITournamentBracketReadModel
    {
        TournamentRunState RunState { get; }
        int CurrentMatchNumber { get; }
        string RoundLabel { get; }
        string StatusLine { get; }
    }
}
