namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderReturnedHomeEvent
    {
        public int SlotId { get; }

        public DefenderReturnedHomeEvent(int slotId)
        {
            SlotId = slotId;
        }
    }
}
