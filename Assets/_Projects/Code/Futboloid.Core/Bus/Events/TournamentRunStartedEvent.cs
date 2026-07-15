namespace Futboloid.Core.Bus.Events
{
    public readonly struct TournamentRunStartedEvent
    {
        public int MatchesToWin { get; }
        public int RunSeed { get; }
        public int StartMatchNumber { get; }

        public TournamentRunStartedEvent(int matchesToWin, int runSeed, int startMatchNumber)
        {
            MatchesToWin = matchesToWin;
            RunSeed = runSeed;
            StartMatchNumber = startMatchNumber;
        }
    }
}
