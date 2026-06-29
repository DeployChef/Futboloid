using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Physics;
using UnityEngine;

namespace Futboloid.Gameplay.Ball
{
    public class BallMotion
    {
        private readonly BallSettings _settings;
        private readonly IGameEventBus _bus;
        private readonly LayerMask _contactMask;
        private readonly LayerMask _goalMask;

        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Speed { get; private set; }

        public bool InPlay => Speed > 0.01f;
        public bool IsHeld => _holdAnchor != null;

        private IBallAnchor _holdAnchor;

        public BallMotion(BallSettings settings, IGameEventBus bus)
        {
            _settings = settings;
            _bus = bus;
            _contactMask = PhysicsLayers.BallContactMask;
            _goalMask = PhysicsLayers.GoalMask;
        }

        public void ResetAt(Vector2 position)
        {
            _holdAnchor = null;
            Position = position;
            Direction = Vector2.zero;
            Speed = 0f;
        }

        /// <summary>Ведение: мяч следует за якорем до ReleaseFromAnchor.</summary>
        public void AttachToAnchor(IBallAnchor anchor)
        {
            _holdAnchor = anchor;
            Speed = 0f;
            Direction = Vector2.zero;
            Position = anchor.WorldPosition;
        }

        public void ReleaseFromAnchor()
        {
            _holdAnchor = null;
        }

        public void Serve(Vector2 position, Vector2 direction)
        {
            _holdAnchor = null;
            Position = position;
            Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
            Speed = _settings.ServeSpeed;
            Direction = ClampMinAngle(Direction);
            _bus.Publish(new BallServedEvent());
        }

        public void Tick(float deltaTime)
        {
            if (_holdAnchor != null)
            {
                Position = _holdAnchor.WorldPosition;
                return;
            }

            if (!InPlay)
                return;

            var distance = Speed * deltaTime;
            var castDistance = distance + _settings.Skin;
            var hit = Physics2D.CircleCast(
                Position,
                _settings.Radius,
                Direction,
                castDistance,
                _contactMask);

            if (hit.collider != null)
                ResolveHit(hit);
            else
                Position += Direction * distance;

            if (TryScoreGoal())
                return;

            Speed = Mathf.MoveTowards(Speed, _settings.BaseSpeed, _settings.Deceleration * deltaTime);
        }

        private void ResolveHit(RaycastHit2D hit)
        {
            Position = hit.point + hit.normal * (_settings.Radius + _settings.Skin);

            if (hit.collider.gameObject.layer == PhysicsLayers.KeeperId)
            {
                Direction = ClampMinAngle(Reflect(Direction, hit.normal));
                Speed = Mathf.Min(Speed + _settings.KeeperBoost, _settings.MaxSpeed);
                _bus.Publish(new BallReturnedToKeeperEvent());
                return;
            }

            Direction = ClampMinAngle(Reflect(Direction, hit.normal));
        }

        private bool TryScoreGoal()
        {
            var hits = Physics2D.OverlapCircleAll(Position, _settings.Radius, _goalMask);
            foreach (var collider in hits)
            {
                if (collider == null)
                    continue;

                var layer = collider.gameObject.layer;
                if (layer == PhysicsLayers.GoalEnemyId)
                {
                    Stop();
                    _bus.Publish(new GoalScoredEvent(isPlayerGoal: true));
                    return true;
                }

                if (layer == PhysicsLayers.GoalPlayerId)
                {
                    Stop();
                    _bus.Publish(new GoalScoredEvent(isPlayerGoal: false));
                    return true;
                }
            }

            return false;
        }

        private void Stop()
        {
            Speed = 0f;
            Direction = Vector2.zero;
        }

        private Vector2 ClampMinAngle(Vector2 direction)
        {
            if (Mathf.Abs(direction.y) >= _settings.MinVerticalComponent)
                return direction;

            var sign = direction.y >= 0f ? 1f : -1f;
            if (Mathf.Abs(direction.y) < 0.001f)
                sign = 1f;

            direction.y = sign * _settings.MinVerticalComponent;
            return direction.normalized;
        }

        private static Vector2 Reflect(Vector2 direction, Vector2 normal)
        {
            return (direction - 2f * Vector2.Dot(direction, normal) * normal).normalized;
        }
    }
}
