namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderPromotionStartedEvent
    {
        public int SlotId { get; }

        public DefenderPromotionStartedEvent(int slotId) => SlotId = slotId;
    }
}
