using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Run;
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
        private readonly IRunProgressionService _runProgression;
        private readonly ContactFilter2D _ballContactFilter;
        private readonly RaycastHit2D[] _castHits = new RaycastHit2D[1];

        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Speed { get; private set; }

        public bool IsOnFire => Speed >= _settings.FireSpeedThreshold;
        public int HitDamage => IsOnFire ? 1 + _settings.FireExtraDamage : 1;

        public bool InPlay => Speed > 0.01f;
        public bool IsHeld => _holdAnchor != null;

        private IBallAnchor _holdAnchor;
        private int _ghostChargesRemaining;

        public BallMotion(
            BallSettings settings,
            IGameEventBus bus,
            DefenderGridRegistry defenderRegistry,
            PitchBounds pitchBounds,
            IRunProgressionService runProgression = null)
        {
            _settings = settings;
            _bus = bus;
            _defenderRegistry = defenderRegistry;
            _pitchBounds = pitchBounds;
            _runProgression = runProgression;

            _ballContactFilter = ContactFilter2D.noFilter;
            _ballContactFilter.SetLayerMask(PhysicsLayers.BallContactMask);
        }

        public void ResetAt(Vector2 position)
        {
            _holdAnchor = null;
            Position = position;
            Direction = Vector2.zero;
            Speed = 0f;
            _ghostChargesRemaining = 0;
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
            var kickMul = _runProgression?.GetGoalkeeperKickMultiplier() ?? 1f;
            Speed = _settings.ServeSpeed * kickMul;
            Direction = ClampMinAngle(Direction);
            RefillGhostCharges();
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

        private void ReflectFromKeeperHit(RaycastHit2D hit)
        {
            var reflected = Reflect(Direction, hit.normal);
            var spread = _settings.KeeperReflectionSpread;

            if (spread > 0f)
            {
                var angle = Mathf.Atan2(reflected.y, reflected.x) * Mathf.Rad2Deg;
                angle += Random.Range(-spread, spread);
                var radians = angle * Mathf.Deg2Rad;
                reflected = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            }

            Direction = ClampMinAngle(reflected);
        }

        public void ApplyKeeperBoost()
        {
            var kickMul = _runProgression?.GetGoalkeeperKickMultiplier() ?? 1f;
            Speed = Mathf.Min(Speed + _settings.KeeperBoost * kickMul, _settings.MaxSpeed);
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
            var hitCollider = hit.collider;
            if (hitCollider == null)
                return;

            var layer = hitCollider.gameObject.layer;
            if (layer == PhysicsLayers.KeeperId)
            {
                Position = hit.point + hit.normal * (_settings.Radius + _settings.Skin);
                ReflectFromKeeperHit(hit);
                ApplyKeeperBoost();
                RefillGhostCharges();
                _bus.Publish(new BallReturnedToKeeperEvent());
                _bus.Publish(new BallContactEvent(BallContactKind.PlayerKeeper, hit.point, hit.normal, Speed));
                return;
            }

            if (layer == PhysicsLayers.DefenderId
                && _defenderRegistry != null
                && _defenderRegistry.TryGetDefender(hitCollider, out var defender))
            {
                if (TryGhostPass(defender, hit))
                    return;

                Position = hit.point + hit.normal * (_settings.Radius + _settings.Skin);
                defender.HandleBallContact(this, hit);
                _bus.Publish(new BallContactEvent(
                    BallContactKind.Defender, hit.point, hit.normal, Speed, defender.SlotId));
                return;
            }

            Position = hit.point + hit.normal * (_settings.Radius + _settings.Skin);
            ReflectFromHit(hit);
            ApplyWallSpeedPenalty();
            _bus.Publish(new BallContactEvent(BallContactKind.Wall, hit.point, hit.normal, Speed));
        }

        private bool TryGhostPass(DefenderView defender, RaycastHit2D hit)
        {
            if (defender == null
                || defender.Role == DefenderRole.Goalkeeper
                || _ghostChargesRemaining <= 0)
                return false;

            if (!defender.TryApplyGhostPassHit())
                return false;

            _ghostChargesRemaining--;
            PassThroughCollider(hit);
            _bus.Publish(new BallContactEvent(
                BallContactKind.Defender, hit.point, hit.normal, Speed, defender.SlotId));
            return true;
        }

        private void PassThroughCollider(RaycastHit2D hit)
        {
            if (hit.collider == null || Direction.sqrMagnitude < 0.0001f)
            {
                Position = hit.point + hit.normal * (_settings.Radius + _settings.Skin);
                return;
            }

            var bounds = hit.collider.bounds;
            var dir = Direction.normalized;
            var halfAlong = Mathf.Abs(bounds.extents.x * dir.x) + Mathf.Abs(bounds.extents.y * dir.y);
            Position = (Vector2)bounds.center
                + dir * (halfAlong + _settings.Radius + _settings.Skin + 0.05f);
        }

        private void RefillGhostCharges() =>
            _ghostChargesRemaining = _runProgression?.GetGhostBallCharges() ?? 0;

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
