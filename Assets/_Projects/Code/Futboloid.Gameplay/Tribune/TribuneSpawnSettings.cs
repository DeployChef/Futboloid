using UnityEngine;

namespace Futboloid.Gameplay.Tribune
{
    [CreateAssetMenu(fileName = "TribuneSpawnSettings", menuName = "Futboloid/Tribune Spawn Settings")]
    public class TribuneSpawnSettings : ScriptableObject
    {
        public const string ResourcePath = "Data/Settings/TribuneSpawnSettings";

        [SerializeField] private float spawnIntervalSeconds = 4f;
        [SerializeField] private float spawnIntervalJitter = 1f;
        [SerializeField] private float minSpawnIntervalSeconds = 2f;
        [SerializeField] private float horizontalSpawnOffset = 1.5f;
        [SerializeField] private float spawnMinY = 4f;
        [SerializeField] private float spawnMaxY = 7.5f;
        [SerializeField] private float targetMinY = -7.5f;
        [SerializeField] private float targetMaxY = -6f;
        [SerializeField] private float targetSpreadX = 2f;
        [SerializeField] private float targetYOffsetMin = -0.4f;
        [SerializeField] private float targetYOffsetMax = 0.6f;
        [SerializeField] private float flightDurationSeconds = 5f;
        [SerializeField] private float flightVisualScale = 0.35f;
        [SerializeField] private float launchHeight = 5f;
        [SerializeField] private float launchHorizontalBias = 0.2f;
        [SerializeField] private float overshootPastTarget = 3.5f;
        [SerializeField] private float catchRadius = 1.4f;

        public float SpawnIntervalSeconds => Mathf.Max(0.5f, spawnIntervalSeconds);
        public float SpawnIntervalJitter => Mathf.Max(0f, spawnIntervalJitter);
        public float MinSpawnIntervalSeconds => Mathf.Max(0.5f, minSpawnIntervalSeconds);
        public float HorizontalSpawnOffset => Mathf.Max(0.1f, horizontalSpawnOffset);
        public float SpawnMinY => spawnMinY;
        public float SpawnMaxY => spawnMaxY;
        public float TargetMinY => targetMinY;
        public float TargetMaxY => targetMaxY;
        public float TargetSpreadX => Mathf.Max(0.1f, targetSpreadX);
        public float TargetYOffsetMin => targetYOffsetMin;
        public float TargetYOffsetMax => targetYOffsetMax;
        public float FlightDurationSeconds => Mathf.Max(0.5f, flightDurationSeconds);
        public float FlightVisualScale => Mathf.Max(0.05f, flightVisualScale);
        public float LaunchHeight => launchHeight;
        public float LaunchHorizontalBias => Mathf.Clamp01(launchHorizontalBias);
        public float OvershootPastTarget => Mathf.Max(0f, overshootPastTarget);
        public float CatchRadius => Mathf.Max(0.2f, catchRadius);

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
