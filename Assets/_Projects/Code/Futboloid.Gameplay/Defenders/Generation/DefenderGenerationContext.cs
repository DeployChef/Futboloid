namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Вход генерации: номер матча, длина забега, сид и модификаторы (перки — позже).</summary>
    public readonly struct DefenderGenerationContext
    {
        public int MatchNumber { get; }
        public int TotalMatches { get; }
        public float EnemyHpMultiplier { get; }
        public int RunSeed { get; }

        public DefenderGenerationContext(
            int matchNumber,
            int totalMatches,
            float enemyHpMultiplier = 1f,
            int runSeed = 0)
        {
            MatchNumber = matchNumber < 1 ? 1 : matchNumber;
            TotalMatches = totalMatches < 1 ? 1 : totalMatches;
            EnemyHpMultiplier = enemyHpMultiplier < 0.1f ? 0.1f : enemyHpMultiplier;
            RunSeed = runSeed;
        }
    }
}
