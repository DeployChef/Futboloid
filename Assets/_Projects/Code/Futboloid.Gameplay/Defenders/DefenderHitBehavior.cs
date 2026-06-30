using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    [CreateAssetMenu(fileName = "DefenderHit_Reflect", menuName = "Futboloid/Defenders/Hit Behavior")]
    public class DefenderHitBehavior : ScriptableObject
    {
        [SerializeField] private DefenderHitType hitType = DefenderHitType.Reflect;
        [SerializeField] private DefenderHitType fallbackWhenNoPass = DefenderHitType.Reflect;
        [SerializeField] private float launchSpeed = 12f;
        [SerializeField] private float openGoalWeight = 0.7f;
        [SerializeField] private float atKeeperWeight = 0.3f;

        public DefenderHitType HitType => hitType;
        public DefenderHitType FallbackWhenNoPass => fallbackWhenNoPass;
        public float LaunchSpeed => launchSpeed;
        public float OpenGoalWeight => openGoalWeight;
        public float AtKeeperWeight => atKeeperWeight;
    }
}
