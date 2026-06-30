using UnityEngine;
using UnityEngine.Serialization;

namespace Futboloid.Gameplay.Defenders
{
    [CreateAssetMenu(fileName = "DefenderHit_Reflect", menuName = "Futboloid/Defenders/Hit Behavior")]
    public class DefenderHitBehavior : ScriptableObject
    {
        [SerializeField] private DefenderHitType hitType = DefenderHitType.Reflect;
        [SerializeField] private float launchSpeed = 12f;

        [Header("ToPlayerGoal")]
        [Tooltip("Шанс 0–100 пнуть в пустую зону ворот. Иначе — в вратаря игрока.")]
        [FormerlySerializedAs("openGoalWeight")]
        [SerializeField] [Range(0, 100)] private int openGoalChancePercent = 70;

        public DefenderHitType HitType => hitType;
        public float LaunchSpeed => launchSpeed;
        public int OpenGoalChancePercent => openGoalChancePercent;
    }
}
