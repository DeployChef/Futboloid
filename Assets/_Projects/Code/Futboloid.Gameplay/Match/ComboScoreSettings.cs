using System;
using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    [Serializable]
    public sealed class ComboScoreSettings
    {
        [SerializeField] private int goalBonusPoints = 150;
        [SerializeField] private int minMultiplier = 1;
        [SerializeField] private int maxMultiplier;
        [SerializeField] private int keeperTouchPenalty = 3;
        [SerializeField] private float decayIntervalSeconds = 2f;
        [SerializeField] private int decayStep = 1;

        public int GoalBonusPoints => Mathf.Max(0, goalBonusPoints);
        public int MinMultiplier => Mathf.Max(1, minMultiplier);
        public int MaxMultiplier => Mathf.Max(0, maxMultiplier);
        public int KeeperTouchPenalty => Mathf.Max(1, keeperTouchPenalty);
        public float DecayIntervalSeconds => Mathf.Max(0.1f, decayIntervalSeconds);
        public int DecayStep => Mathf.Max(1, decayStep);
    }
}
