namespace Futboloid.Core.Bus.Events
{
    public readonly struct PerkPickedEvent
    {
        public string PerkId { get; }
        public int NewLevel { get; }

        public PerkPickedEvent(string perkId, int newLevel)
        {
            PerkId = perkId;
            NewLevel = newLevel;
        }
    }
}
