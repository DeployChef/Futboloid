using UnityEngine;

namespace Futboloid.Gameplay.Tribune
{
    [CreateAssetMenu(fileName = "TribuneSpawnSettings", menuName = "Futboloid/Tribune Spawn Settings")]
    public class TribuneSpawnSettings : ScriptableObject
    {
        public const string ResourcePath = "Data/Settings/TribuneSpawnSettings";

        [SerializeField] private float spawnIntervalSeconds = 10f;
        [SerializeField] private float spawnIntervalJitter = 3f;
        [SerializeField] private float horizontalSpawnOffset = 1.5f;
        [SerializeField] private float spawnMinY = 4f;
        [SerializeField] private float spawnMaxY = 7.5f;
        [SerializeField] private float targetMinY = -6f;
        [SerializeField] private float targetMaxY = -2f;
        [SerializeField] private float flightDurationSeconds = 2.5f;
        [SerializeField] private float arcHeight = 2.5f;

        public float SpawnIntervalSeconds => Mathf.Max(0.5f, spawnIntervalSeconds);
        public float SpawnIntervalJitter => Mathf.Max(0f, spawnIntervalJitter);
        public float HorizontalSpawnOffset => Mathf.Max(0.1f, horizontalSpawnOffset);
        public float SpawnMinY => spawnMinY;
        public float SpawnMaxY => spawnMaxY;
        public float TargetMinY => targetMinY;
        public float TargetMaxY => targetMaxY;
        public float FlightDurationSeconds => Mathf.Max(0.5f, flightDurationSeconds);
        public float ArcHeight => arcHeight;

        public static TribuneSpawnSettings Load()
        {
            var settings = Resources.Load<TribuneSpawnSettings>(ResourcePath);
            if (settings != null)
                return settings;

            Debug.LogWarning(
                $"[TribuneSpawnSettings] Asset not found at Resources/{ResourcePath}. " +
                "Using runtime defaults.");
            return CreateInstance<TribuneSpawnSettings>();
        }
    }
}
