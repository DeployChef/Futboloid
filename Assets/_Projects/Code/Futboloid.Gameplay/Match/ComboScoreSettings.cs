using System;
using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    [Serializable]
    public sealed class ComboScoreSettings
    {
        [SerializeField] private int goalBonusPoints = 50;
        [SerializeField] private int maxMultiplier;

        public int GoalBonusPoints => Mathf.Max(0, goalBonusPoints);
        public int MaxMultiplier => Mathf.Max(0, maxMultiplier);
    }
}
