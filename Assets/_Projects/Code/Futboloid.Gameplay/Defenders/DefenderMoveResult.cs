using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    public readonly struct DefenderMoveResult
    {
        private const float MoveThresholdSq = 0.000001f;

        public DefenderMoveResult(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        public Vector2 Position { get; }
        public Vector2 Velocity { get; }
        public bool IsMoving => Velocity.sqrMagnitude > MoveThresholdSq;
    }
}
