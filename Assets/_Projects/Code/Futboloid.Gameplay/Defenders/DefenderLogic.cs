using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Keeper;
using Futboloid.Gameplay.Physics;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    public sealed class DefenderLogic
    {
        private readonly GoalkeeperView _playerKeeper;
        private float _goalkeeperParam;
        private Vector2 _runVelocity;

        public DefenderLogic(GoalkeeperView playerKeeper)
        {
            _playerKeeper = playerKeeper;
        }

        public void ResetRunVelocity() => _runVelocity = Vector2.zero;

        public Vector2 TickRunTowards(
            Vector2 current,
            Vector2 target,
            float maxSpeed,
            float acceleration,
            float arriveThreshold,
            float deltaTime,
            out bool arrived)
        {
            var offset = target - current;
            var distSqr = offset.sqrMagnitude;
            if (distSqr <= arriveThreshold * arriveThreshold)
            {
                arrived = true;
                _runVelocity = Vector2.zero;
                return target;
            }

            arrived = false;

            var desiredVelocity = offset.normalized * maxSpeed;
            _runVelocity = Vector2.MoveTowards(_runVelocity, desiredVelocity, acceleration * deltaTime);
            var next = current + _runVelocity * deltaTime;

            var remaining = target - next;
            if (Vector2.Dot(offset, remaining) <= 0f)
            {
                _runVelocity = Vector2.zero;
                arrived = true;
                return target;
            }

            return next;
        }

        public Vector2 TickGoalkeeperOnParabola(
            GoalAnchor zone,
            float ballWorldX,
            float trackSpeed,
            float deltaTime)
        {
            if (zone == null)
                return Vector2.zero;

            var targetT = zone.ParamFromWorldX(ballWorldX);
            var speed = Mathf.Max(0.01f, trackSpeed);
            _goalkeeperParam = Mathf.MoveTowards(_goalkeeperParam, targetT, speed * deltaTime);
            return zone.PositionOnParabola(_goalkeeperParam);
        }

        public void ResetGoalkeeperParam(float startParam)
        {
            _goalkeeperParam = Mathf.Clamp(startParam, -1f, 1f);
        }

        public void ResolveBallHit(BallMotion motion, RaycastHit2D hit, DefenderView view)
        {
            if (motion == null || view == null)
                return;

            switch (view.HitType)
            {
                case DefenderHitType.Reflect:
                    motion.ReflectFromHit(hit);
                    break;

                case DefenderHitType.ToPlayerGoal:
                    LaunchToPlayerGoal(motion, hit, view);
                    break;

                default:
                    motion.ReflectFromHit(hit);
                    break;
            }
        }

        private void LaunchToPlayerGoal(BallMotion motion, RaycastHit2D hit, DefenderView view)
        {
            var origin = HitOrigin(hit, view);
            var target = ResolvePlayerGoalTarget(view);
            var delta = target - origin;

            if (delta.sqrMagnitude < 0.0001f)
            {
                motion.LaunchDirected(Vector2.down, view.LaunchSpeed);
                return;
            }

            motion.LaunchDirected(delta, view.LaunchSpeed);
        }

        private Vector2 ResolvePlayerGoalTarget(DefenderView view)
        {
            if (_playerKeeper == null)
                return ResolvePlayerGoalFallback();

            var keeperPos = (Vector2)_playerKeeper.transform.position;
            if (!TryGetPlayerGoalBounds(out var goalBounds))
                goalBounds = BuildFallbackGoalBounds();

            var inset = 0.25f;
            var oppositeCornerX = keeperPos.x <= goalBounds.center.x
                ? goalBounds.max.x - inset
                : goalBounds.min.x + inset;
            var oppositeCorner = new Vector2(oppositeCornerX, goalBounds.center.y);

            var aimFar = Random.Range(0, 100) < view.OpenGoalChancePercent;
            var distanceFromKeeper = aimFar
                ? Random.Range(0.7f, 1f)
                : Random.Range(0.08f, 0.35f);

            return Vector2.Lerp(keeperPos, oppositeCorner, distanceFromKeeper);
        }

        private static bool TryGetPlayerGoalBounds(out Bounds bounds)
        {
            var colliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider != null && collider.gameObject.layer == PhysicsLayers.GoalPlayerId)
                {
                    bounds = collider.bounds;
                    return true;
                }
            }

            bounds = default;
            return false;
        }

        private static Bounds BuildFallbackGoalBounds()
        {
            const float halfWidth = 4.46f;
            const float goalY = -8.3f;
            var center = new Vector3(0f, goalY, 0f);
            var size = new Vector3(halfWidth * 2f, 0.5f, 0f);
            return new Bounds(center, size);
        }

        private static Vector2 ResolvePlayerGoalFallback()
        {
            if (TryGetPlayerGoalBounds(out var goalBounds))
                return goalBounds.center;

            return new Vector2(0f, -8.3f);
        }

        private static Vector2 HitOrigin(RaycastHit2D hit, DefenderView view)
        {
            if (hit.collider != null)
                return hit.point;

            return view.transform.position;
        }
    }
}
