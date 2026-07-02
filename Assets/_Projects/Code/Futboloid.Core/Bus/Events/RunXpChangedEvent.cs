namespace Futboloid.Core.Bus.Events
{
    public readonly struct RunXpChangedEvent
    {
        public int CurrentXp { get; }
        public int XpToNextLevel { get; }
        public float Fill01 { get; }

        public RunXpChangedEvent(int currentXp, int xpToNextLevel, float fill01)
        {
            CurrentXp = currentXp;
            XpToNextLevel = xpToNextLevel;
            Fill01 = fill01;
        }
    }
}
