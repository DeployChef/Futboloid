namespace Futboloid.Core
{
    public interface ITournamentBracketReadModel
    {
        TournamentRunState RunState { get; }
        string RoundLabel { get; }
        string StatusLine { get; }
    }
}
