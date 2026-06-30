namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderDestroyedEvent
    {
        public int SlotId { get; }

        public DefenderDestroyedEvent(int slotId) => SlotId = slotId;
    }
}
