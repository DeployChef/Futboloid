using UnityEngine;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderDamagedEvent
    {
        public int SlotId { get; }
        public int RemainingHp { get; }
        public Vector2 WorldPosition { get; }

        public DefenderDamagedEvent(int slotId, int remainingHp, Vector2 worldPosition)
        {
            SlotId = slotId;
            RemainingHp = remainingHp;
            WorldPosition = worldPosition;
        }
    }
}
