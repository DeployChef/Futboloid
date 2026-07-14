namespace Futboloid.Core.Bus.Events
{
    /// <summary>
    /// Публикуется при повышении уровня персонажа в забеге.
    /// Стреляет один раз на каждый полученный уровень.
    /// </summary>
    public readonly struct LevelUpEvent
    {
        public int NewLevel { get; }

        public LevelUpEvent(int newLevel)
        {
            NewLevel = newLevel;
        }
    }
}
