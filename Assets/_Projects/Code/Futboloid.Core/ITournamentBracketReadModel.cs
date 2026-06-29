namespace Futboloid.Core
{
    public interface ITournamentBracketReadModel
    {
        string RoundLabel { get; }
        string StatusLine { get; }
        bool CanStartNextMatch { get; }
        bool IsEliminated { get; }
        bool IsChampion { get; }
    }
}
