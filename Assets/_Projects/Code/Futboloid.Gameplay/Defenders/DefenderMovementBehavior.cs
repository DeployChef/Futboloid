using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    [CreateAssetMenu(fileName = "DefenderMovement_Idle", menuName = "Futboloid/Defenders/Movement Behavior")]
    public class DefenderMovementBehavior : ScriptableObject
    {
        [SerializeField] private DefenderMovementType movementType = DefenderMovementType.Idle;

        [Header("Patrol")]
        [SerializeField] private int patrolPointCount = 4;
        [SerializeField] private float patrolRadius = 1.5f;

        [Header("Wander / Chase")]
        [SerializeField] private float wanderRadius = 1.5f;
        [SerializeField] private float chaseRadius = 3f;

        [Header("Separation")]
        [SerializeField] private float separationRadius = 0.6f;

        public DefenderMovementType MovementType => movementType;
        public int PatrolPointCount => patrolPointCount;
        public float PatrolRadius => patrolRadius;
        public float WanderRadius => wanderRadius;
        public float ChaseRadius => chaseRadius;
        public float SeparationRadius => separationRadius;
    }
}
