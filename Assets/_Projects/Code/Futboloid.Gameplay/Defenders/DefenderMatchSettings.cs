using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    [CreateAssetMenu(fileName = "DefenderMatchSettings", menuName = "Futboloid/Defender Match Settings")]
    public sealed class DefenderMatchSettings : ScriptableObject
    {
        public const string ResourcePath = "Data/Settings/DefenderMatchSettings";

        [Header("Goalkeeper promotion")]
        [SerializeField] private float runToGoalSpeed = 4f;
        [SerializeField] private float runToGoalAcceleration = 18f;
        [SerializeField] private float arriveThreshold = 0.08f;

        [Header("Reshuffle after goal")]
        [SerializeField] private float reshuffleMoveDuration = 0.55f;

        public float RunToGoalSpeed => Mathf.Max(0.01f, runToGoalSpeed);
        public float RunToGoalAcceleration => Mathf.Max(0.01f, runToGoalAcceleration);
        public float ArriveThreshold => Mathf.Max(0.001f, arriveThreshold);
        public float ReshuffleMoveDuration => Mathf.Max(0.01f, reshuffleMoveDuration);

        public static DefenderMatchSettings Load()
        {
            var settings = Resources.Load<DefenderMatchSettings>(ResourcePath);
            if (settings != null)
                return settings;

            Debug.LogWarning(
                $"[DefenderMatchSettings] Asset not found at Resources/{ResourcePath}. " +
                "Using runtime defaults. Create via Assets → Create → Futboloid → Defender Match Settings.");

            return CreateInstance<DefenderMatchSettings>();
        }
    }
}
