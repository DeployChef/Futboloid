using Futboloid.Gameplay.Defenders;
using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    [CreateAssetMenu(fileName = "GameplaySettings", menuName = "Futboloid/Gameplay Settings")]
    public class GameplaySettings : ScriptableObject
    {
        public const string ResourcePath = "Data/Settings/GameplaySettings";
        public const float DefaultMatchDurationSeconds = 90f;
        public const int DefaultMatchesToWin = 3;

        [SerializeField] private float matchDurationSeconds = DefaultMatchDurationSeconds;
        [SerializeField] private int matchesToWin = DefaultMatchesToWin;
        [SerializeField] private DefenderGenerationSettings defenderGeneration;
        [SerializeField] private DefenderMatchSettings defenderMatch;
        [SerializeField] private ComboScoreSettings comboScore = new();

        public float MatchDurationSeconds => Mathf.Max(1f, matchDurationSeconds);
        public int MatchesToWin => Mathf.Max(1, matchesToWin);
        public DefenderGenerationSettings DefenderGeneration =>
            defenderGeneration != null
                ? defenderGeneration
                : DefenderGenerationSettings.Load();
        public DefenderMatchSettings DefenderMatch =>
            defenderMatch != null
                ? defenderMatch
                : DefenderMatchSettings.Load();
        public ComboScoreSettings ComboScore => comboScore ??= new ComboScoreSettings();

        public static GameplaySettings Load()
        {
            var settings = Resources.Load<GameplaySettings>(ResourcePath);
            if (settings != null)
                return settings;

            Debug.LogWarning(
                $"[GameplaySettings] Asset not found at Resources/{ResourcePath}. " +
                "Using defaults. Create via Assets → Create → Futboloid → Gameplay Settings.");

            return CreateInstance<GameplaySettings>();
        }
    }
}
