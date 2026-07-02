namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderDestroyedEvent
    {
        public int SlotId { get; }
        public bool WasGoalkeeper { get; }

        public DefenderDestroyedEvent(int slotId, bool wasGoalkeeper)
        {
            SlotId = slotId;
            WasGoalkeeper = wasGoalkeeper;
        }
    }
}
