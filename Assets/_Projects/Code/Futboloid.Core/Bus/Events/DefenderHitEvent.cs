using UnityEngine;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct DefenderHitEvent
    {
        public int SlotId { get; }
        public bool IsPass { get; }
        public int TargetSlotId { get; }

        public DefenderHitEvent(int slotId, bool isPass = false, int targetSlotId = -1)
        {
            SlotId = slotId;
            IsPass = isPass;
            TargetSlotId = targetSlotId;
        }
    }
}
