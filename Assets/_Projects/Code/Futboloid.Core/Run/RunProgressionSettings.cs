using UnityEngine;

namespace Futboloid.Core.Run
{
    [CreateAssetMenu(fileName = "RunProgressionSettings", menuName = "Futboloid/Run Progression Settings")]
    public class RunProgressionSettings : ScriptableObject
    {
        public const string ResourcePath = "Data/Settings/RunProgressionSettings";

        public const int DefaultXpPerHit = 2;
        public const int DefaultXpPerKill = 12;
        public const int DefaultBaseXpToLevel = 30;
        public const int DefaultXpPerLevelStep = 10;
        public const int DefaultOfferCount = 3;

        [SerializeField] private int xpPerHit = DefaultXpPerHit;
        [SerializeField] private int xpPerKill = DefaultXpPerKill;
        [SerializeField] private int baseXpToLevel = DefaultBaseXpToLevel;
        [SerializeField] private int xpPerLevelStep = DefaultXpPerLevelStep;
        [SerializeField] private int offerCount = DefaultOfferCount;

        public int XpPerHit => Mathf.Max(0, xpPerHit);
        public int XpPerKill => Mathf.Max(0, xpPerKill);
        public int BaseXpToLevel => Mathf.Max(1, baseXpToLevel);
        public int XpPerLevelStep => Mathf.Max(0, xpPerLevelStep);
        public int OfferCount => Mathf.Clamp(offerCount, 1, 3);

        public int XpRequiredForLevel(int runLevel) =>
            BaseXpToLevel + Mathf.Max(0, runLevel - 1) * XpPerLevelStep;

        public static RunProgressionSettings Load()
        {
            var settings = Resources.Load<RunProgressionSettings>(ResourcePath);
            if (settings != null)
                return settings;

            Debug.LogWarning(
                $"[RunProgressionSettings] Asset not found at Resources/{ResourcePath}. " +
                "Using defaults.");
            return CreateInstance<RunProgressionSettings>();
        }
    }
}
