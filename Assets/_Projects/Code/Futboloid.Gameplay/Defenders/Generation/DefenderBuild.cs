using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Параметры одного заспавненного футболиста.</summary>
    public struct DefenderBuild
    {
        public int SlotId;
        public DefenderRole Role;
        public int MaxHp;
        public DefenderHitType HitType;
        public DefenderMovementType MovementType;
        public int PatrolPointCount;
        public float PatrolRadius;
        public float WanderRadius;
        public float SeparationRadius;
        public float FieldMoveSpeed;
        public float FieldAcceleration;
        public float FieldArriveThreshold;
        public float LaunchSpeed;
        public int OpenGoalChancePercent;
        public float InteractionCooldown;
        public float TrackSpeed;
        public int PointValue;
    }
}
