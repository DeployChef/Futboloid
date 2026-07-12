using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Defenders;
using Futboloid.Gameplay.Match;
using Futboloid.Gameplay.Physics;
using UnityEngine;

namespace Futboloid.Gameplay.Ball
{
    public class BallMotion
    {
        private readonly BallSettings _settings;
        private readonly IGameEventBus _bus;
        private readonly DefenderGridRegistry _defenderRegistry;
        private readonly PitchBounds _pitchBounds;
        private readonly ContactFilter2D _ballContactFilter;
        private readonly RaycastHit2D[] _castHits = new RaycastHit2D[1];

        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Speed { get; private set; }

        public bool InPlay => Speed > 0.01f;
        public bool IsHeld => _holdAnchor != null;

        private IBallAnchor _holdAnchor;

        public BallMotion(
            BallSettings settings,
            IGameEventBus bus,
            DefenderGridRegistry defenderRegistry,
            PitchBounds pitchBounds)
        {
            _settings = settings;
            _bus = bus;
            _defenderRegistry = defenderRegistry;
            _pitchBounds = pitchBounds;

            _ballContactFilter = ContactFilter2D.noFilter;
            _ballContactFilter.SetLayerMask(PhysicsLayers.BallContactMask);
        }

        public void ResetAt(Vector2 position)
        {
            _holdAnchor = null;
            Position = position;
            Direction = Vector2.zero;
            Speed = 0f;
        }

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
            var hitCount = Physics2D.CircleCast(
                Position,
                _settings.Radius,
                Direction,
                _ballContactFilter,
                _castHits,
                castDistance);

            if (hitCount > 0)
                ResolveHit(_castHits[0]);
            else
                Position += Direction * distance;

            if (TryScoreGoal())
                return;

            Speed = Mathf.MoveTowards(Speed, _settings.BaseSpeed, _settings.Deceleration * deltaTime);
            RecoverIfFarOutsideBounds();
        }

        private void RecoverIfFarOutsideBounds()
        {
            if (_pitchBounds == null)
                return;

            if (_pitchBounds.DistanceOutside(Position) < _pitchBounds.BallRecoveryOverflow)
                return;

            Position = _pitchBounds.Center;
            Direction = ClampMinAngle(Vector2.up);
            Speed = _settings.BaseSpeed;
        }

        public void ReflectFromHit(RaycastHit2D hit)
        {
            Direction = ClampMinAngle(Reflect(Direction, hit.normal));
        }

        public void ApplyKeeperBoost()
        {
            Speed = Mathf.Min(Speed + _settings.KeeperBoost, _settings.MaxSpeed);
        }

        public void ApplyDefenderHitBoost()
        {
            Speed = Mathf.Min(Speed + _settings.DefenderHitBoost, _settings.MaxSpeed);
        }

        public void ApplyWallSpeedPenalty()
        {
            Speed = Mathf.Max(_settings.BaseSpeed, Speed - _settings.WallSpeedPenalty);
        }

        public void LaunchDirected(Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Direction = ClampMinAngle(direction.normalized);
            Speed = speed;
        }

        private void ResolveHit(RaycastHit2D hit)
        {
            Position = hit.point + hit.normal * (_settings.Radius + _settings.Skin);

            var hitCollider = hit.collider;
            if (hitCollider == null)
                return;

            var layer = hitCollider.gameObject.layer;
            if (layer == PhysicsLayers.KeeperId)
            {
                ReflectFromHit(hit);
                ApplyKeeperBoost();
                _bus.Publish(new BallReturnedToKeeperEvent());
                _bus.Publish(new BallContactEvent(BallContactKind.PlayerKeeper, hit.point, hit.normal, Speed));
                return;
            }

            if (layer == PhysicsLayers.DefenderId
                && _defenderRegistry != null
                && _defenderRegistry.TryGetDefender(hitCollider, out var defender))
            {
                defender.HandleBallContact(this, hit);
                _bus.Publish(new BallContactEvent(
                    BallContactKind.Defender, hit.point, hit.normal, Speed, defender.SlotId));
                return;
            }

            ReflectFromHit(hit);
            ApplyWallSpeedPenalty();
            _bus.Publish(new BallContactEvent(BallContactKind.Wall, hit.point, hit.normal, Speed));
        }

        private bool TryScoreGoal()
        {
            if (Physics2D.OverlapCircle(Position, _settings.Radius, PhysicsLayers.GoalEnemyMask) != null)
            {
                Stop();
                _bus.Publish(new GoalScoredEvent(isPlayerGoal: true));
                return true;
            }

            if (Physics2D.OverlapCircle(Position, _settings.Radius, PhysicsLayers.GoalPlayerMask) != null)
            {
                Stop();
                _bus.Publish(new GoalScoredEvent(isPlayerGoal: false));
                return true;
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
