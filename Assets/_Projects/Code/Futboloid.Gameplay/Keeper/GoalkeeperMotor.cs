using UnityEngine;

namespace Futboloid.Gameplay.Keeper
{
    public readonly struct GoalkeeperMoveResult
    {
        public GoalkeeperMoveResult(Vector2 position, float velocityX)
        {
            Position = position;
            VelocityX = velocityX;
        }

        public Vector2 Position { get; }
        public float VelocityX { get; }
        public bool IsMoving => Mathf.Abs(VelocityX) > 0.001f;
    }

    /// <summary>
    /// Горизонтальное движение вратаря с ускорением и clamp по границам.
    /// </summary>
    public sealed class GoalkeeperMotor
    {
        private float _velocityX;

        public float VelocityX => _velocityX;

        public void ResetVelocity() => _velocityX = 0f;

        public Vector2 SnapToCenterX(Vector2 position, float centerX)
        {
            _velocityX = 0f;
            position.x = centerX;
            return position;
        }

        public GoalkeeperMoveResult Tick(
            Vector2 position,
            float minX,
            float maxX,
            float minY,
            float maxY,
            float moveInput,
            float speed,
            float speedMultiplier,
            float acceleration,
            float deltaTime)
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);

            var effectiveSpeed = speed * speedMultiplier;
            var desiredVelocity = Mathf.Abs(moveInput) < 0.001f ? 0f : moveInput * effectiveSpeed;
            _velocityX = Mathf.MoveTowards(_velocityX, desiredVelocity, acceleration * deltaTime);

            var previousX = position.x;
            position.x = Mathf.Clamp(position.x + _velocityX * deltaTime, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);

            if (position.x <= minX && _velocityX < 0f || position.x >= maxX && _velocityX > 0f)
                _velocityX = 0f;
            else if (Mathf.Abs(position.x - previousX) < 0.0001f && Mathf.Abs(desiredVelocity) > 0.001f)
                _velocityX = 0f;

            return new GoalkeeperMoveResult(position, _velocityX);
        }
    }
}
