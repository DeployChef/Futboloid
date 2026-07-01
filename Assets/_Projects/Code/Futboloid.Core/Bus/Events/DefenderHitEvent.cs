namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderHitEvent
    {
        public int SlotId { get; }

        public DefenderHitEvent(int slotId)
        {
            SlotId = slotId;
        }
    }
}
