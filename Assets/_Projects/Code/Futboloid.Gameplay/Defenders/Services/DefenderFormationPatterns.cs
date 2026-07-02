using System.Collections.Generic;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Паттерны расстановки полевых игроков по номеру матча.</summary>
    public static class DefenderFormationPatterns
    {
        public const int GoalkeeperSlotId = 100;

        public static void CollectFieldSlots(int matchNumber, List<int> output)
        {
            output.Clear();

            // MVP: фиксированные слоты. Позже — сетки/волны по matchNumber.
            output.Add(15);
            output.Add(17);
            output.Add(19);
        }
    }
}
