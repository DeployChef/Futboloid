namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderRoleChangedEvent
    {
        public int SlotId { get; }
        public bool IsGoalkeeper { get; }

        public DefenderRoleChangedEvent(int slotId, bool isGoalkeeper)
        {
            SlotId = slotId;
            IsGoalkeeper = isGoalkeeper;
        }
    }
}
