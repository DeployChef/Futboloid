namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Вход генерации: номер матча и модификаторы забега (перки — позже).</summary>
    public readonly struct DefenderGenerationContext
    {
        public int MatchNumber { get; }
        public float EnemyHpMultiplier { get; }

        public DefenderGenerationContext(int matchNumber, float enemyHpMultiplier = 1f)
        {
            MatchNumber = matchNumber < 1 ? 1 : matchNumber;
            EnemyHpMultiplier = enemyHpMultiplier < 0.1f ? 0.1f : enemyHpMultiplier;
        }
    }
}
