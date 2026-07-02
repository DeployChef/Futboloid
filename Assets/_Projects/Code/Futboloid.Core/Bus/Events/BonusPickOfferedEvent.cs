using Futboloid.Core.Run;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct BonusPickOfferedEvent
    {
        public PerkDefinition Offer0 { get; }
        public PerkDefinition Offer1 { get; }
        public PerkDefinition Offer2 { get; }
        public int LevelAfterPick0 { get; }
        public int LevelAfterPick1 { get; }
        public int LevelAfterPick2 { get; }

        public BonusPickOfferedEvent(
            PerkDefinition offer0,
            PerkDefinition offer1,
            PerkDefinition offer2,
            int levelAfterPick0,
            int levelAfterPick1,
            int levelAfterPick2)
        {
            Offer0 = offer0;
            Offer1 = offer1;
            Offer2 = offer2;
            LevelAfterPick0 = levelAfterPick0;
            LevelAfterPick1 = levelAfterPick1;
            LevelAfterPick2 = levelAfterPick2;
        }

        public int Count =>
            (Offer0 != null ? 1 : 0)
            + (Offer1 != null ? 1 : 0)
            + (Offer2 != null ? 1 : 0);
    }
}
