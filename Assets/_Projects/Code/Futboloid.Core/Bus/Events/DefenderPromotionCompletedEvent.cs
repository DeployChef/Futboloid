namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderPromotionCompletedEvent
    {
        public int SlotId { get; }

        public DefenderPromotionCompletedEvent(int slotId) => SlotId = slotId;
    }
}
