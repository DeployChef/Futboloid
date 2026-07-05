namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderHitEvent
    {
        public int SlotId { get; }
        public int PointValue { get; }

        public DefenderHitEvent(int slotId, int pointValue)
        {
            SlotId = slotId;
            PointValue = pointValue;
        }
    }
}
