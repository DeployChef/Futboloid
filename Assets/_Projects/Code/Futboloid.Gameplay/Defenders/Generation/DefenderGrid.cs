namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Координаты ячейки на сетке 5×7 и преобразование в slot id.</summary>
    public static class DefenderGrid
    {
        public static int ToSlotId(int col, int row, int columns) => row * columns + col;

        public static void FromSlotId(int slotId, int columns, out int col, out int row)
        {
            row = slotId / columns;
            col = slotId % columns;
        }

        public static long PackCell(int col, int row) => ((long)row << 32) | (uint)col;

        public static void UnpackCell(long packed, out int col, out int row)
        {
            col = (int)(packed & 0xFFFFFFFF);
            row = (int)(packed >> 32);
        }
    }
}
