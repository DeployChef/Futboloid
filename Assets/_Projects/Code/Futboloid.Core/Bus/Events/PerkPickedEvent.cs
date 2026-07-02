using Futboloid.Core.Run;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct PerkPickedEvent
    {
        public string PerkId { get; }
        public int NewLevel { get; }
        public PerkCardColor Color { get; }

        public PerkPickedEvent(string perkId, int newLevel, PerkCardColor color)
        {
            PerkId = perkId;
            NewLevel = newLevel;
            Color = color;
        }
    }
}
