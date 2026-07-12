using System.Collections.Generic;
using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Defenders;
using Futboloid.Gameplay.Keeper;
using Futboloid.Gameplay.Match;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    public sealed class DefenderLogic
    {
        private readonly GoalkeeperView _playerKeeper;
        private readonly PitchBounds _pitchBounds;
        private float _goalkeeperParam;
        private Vector2 _runVelocity;
        private Vector2 _fieldVelocity;
        private Vector2[] _patrolPath;
        private int _patrolIndex;
        private Vector2 _wanderTarget;
        private bool _hasWanderTarget;
        private System.Random _wanderRng;

        public DefenderLogic(GoalkeeperView playerKeeper, PitchBounds pitchBounds)
        {
            _playerKeeper = playerKeeper;
            _pitchBounds = pitchBounds;
        }

        public void ResetRunVelocity() => _runVelocity = Vector2.zero;

        public void ResetFieldVelocity() => _fieldVelocity = Vector2.zero;

        public void InitializeFieldMovement(Vector2 home, int slotId, int patrolPointCount, float patrolRadius)
        {
            _fieldVelocity = Vector2.zero;
            _hasWanderTarget = false;
            _wanderRng = new System.Random(slotId * 7919 + 17);
            _patrolPath = PatrolPathGenerator.Generate(home, patrolPointCount, patrolRadius, slotId * 7919 + 17);
            for (var i = 0; i < _patrolPath.Length; i++)
                _patrolPath[i] = ClampToPitch(_patrolPath[i]);
            _patrolIndex = 0;
        }

        public DefenderMoveResult TickFieldMovement(
            Vector2 current,
            DefenderMovementType movementType,
            Vector2 home,
            float wanderRadius,
            Vector2? ballPosition,
            float moveSpeed,
            float acceleration,
            float arriveThreshold,
            float separationRadius,
            IReadOnlyList<Vector2> neighborPositions,
            float deltaTime)
        {
            Vector2 next;
            if (movementType == DefenderMovementType.Idle)
            {
                var idle = MoveTowards(current, ClampToPitch(home), moveSpeed, acceleration, arriveThreshold, deltaTime, ref _fieldVelocity);
                next = ClampToPitch(ApplySeparation(idle, neighborPositions, separationRadius, moveSpeed, deltaTime));
            }
            else
            {
                var target = ResolveFieldTarget(
                    movementType,
                    home,
                    wanderRadius,
                    ballPosition,
                    current,
                    arriveThreshold);

                var moved = MoveTowards(current, target, moveSpeed, acceleration, arriveThreshold, deltaTime, ref _fieldVelocity);
                next = ClampToPitch(ApplySeparation(moved, neighborPositions, separationRadius, moveSpeed, deltaTime));
            }

            return new DefenderMoveResult(next, _fieldVelocity);
        }

        public DefenderMoveResult TickRunTowards(
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
                return new DefenderMoveResult(ClampToPitch(target), _runVelocity);
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
                return new DefenderMoveResult(ClampToPitch(target), _runVelocity);
            }

            return new DefenderMoveResult(ClampToPitch(next), _runVelocity);
        }

        public DefenderMoveResult TickGoalkeeperOnParabola(
            Vector2 current,
            GoalAnchor zone,
            float ballWorldX,
            float trackSpeed,
            float deltaTime)
        {
            if (zone == null)
                return new DefenderMoveResult(current, Vector2.zero);

            var targetT = zone.ParamFromWorldX(ballWorldX);
            var speed = Mathf.Max(0.01f, trackSpeed);
            _goalkeeperParam = Mathf.MoveTowards(_goalkeeperParam, targetT, speed * deltaTime);
            var next = zone.PositionOnParabola(_goalkeeperParam);
            var velocity = deltaTime > 0f ? (next - current) / deltaTime : Vector2.zero;
            return new DefenderMoveResult(next, velocity);
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
                    if (IsHitFromBehind(hit, view))
                        motion.ReflectFromHit(hit);
                    else
                        LaunchToPlayerGoal(motion, hit, view);
                    break;

                default:
                    motion.ReflectFromHit(hit);
                    break;
            }

            motion.ApplyDefenderHitBoost();
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

        /// <summary>
        /// Удар в спину: точка контакта за спиной относительно направления к воротам игрока.
        /// forward = к воротам; угол θ между forward и (центр→контакт):
        /// θ ≤ 110° — бьёт в ворота, θ &gt; 110° — reflect (cos 110° ≈ −0.34).
        /// </summary>
        private const float BackHitDotThreshold = -0.34f;

        private bool IsHitFromBehind(RaycastHit2D hit, DefenderView view)
        {
            var defenderPos = (Vector2)view.transform.position;
            var forward = ResolveDefenderForward(defenderPos);
            if (forward.sqrMagnitude < 0.0001f || hit.collider == null)
                return false;

            var toContact = hit.point - defenderPos;
            if (toContact.sqrMagnitude < 0.0001f)
                return false;

            return Vector2.Dot(toContact.normalized, forward) < BackHitDotThreshold;
        }

        private Vector2 ResolveDefenderForward(Vector2 defenderPosition)
        {
            var goalCenter = (Vector2)GetPlayerGoalBounds().center;
            var forward = goalCenter - defenderPosition;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector2.down;

            return forward.normalized;
        }

        private Vector2 ResolvePlayerGoalTarget(DefenderView view)
        {
            var goalBounds = GetPlayerGoalBounds();

            if (_playerKeeper == null)
                return goalBounds.center;

            var keeperPos = (Vector2)_playerKeeper.transform.position;
            var inset = 0.25f;
            var oppositeCornerX = keeperPos.x <= goalBounds.center.x
                ? goalBounds.max.x - inset
                : goalBounds.min.x + inset;
            var oppositeCorner = new Vector2(oppositeCornerX, goalBounds.center.y);

            var aimFar = UnityEngine.Random.Range(0, 100) < view.OpenGoalChancePercent;
            var distanceFromKeeper = aimFar
                ? UnityEngine.Random.Range(0.7f, 1f)
                : UnityEngine.Random.Range(0.08f, 0.35f);

            return Vector2.Lerp(keeperPos, oppositeCorner, distanceFromKeeper);
        }

        private static Bounds GetPlayerGoalBounds()
        {
            const float halfWidth = 4.46f;
            const float goalY = -8.3f;
            var center = new Vector3(0f, goalY, 0f);
            var size = new Vector3(halfWidth * 2f, 0.5f, 0f);
            return new Bounds(center, size);
        }

        private static Vector2 HitOrigin(RaycastHit2D hit, DefenderView view)
        {
            if (hit.collider != null)
                return hit.point;

            return view.transform.position;
        }

        private Vector2 ResolveFieldTarget(
            DefenderMovementType movementType,
            Vector2 home,
            float wanderRadius,
            Vector2? ballPosition,
            Vector2 current,
            float arriveThreshold)
        {
            switch (movementType)
            {
                case DefenderMovementType.PatrolGenerated:
                    return ResolvePatrolTarget(current, arriveThreshold);

                case DefenderMovementType.WanderInRadius:
                    return ResolveWanderTarget(home, wanderRadius, current, arriveThreshold);

                case DefenderMovementType.ChaseBall:
                    if (ballPosition.HasValue)
                        return ClampToPitch(ballPosition.Value);

                    return ResolveWanderTarget(home, wanderRadius, current, arriveThreshold);

                default:
                    return home;
            }
        }

        private Vector2 ResolvePatrolTarget(Vector2 current, float arriveThreshold)
        {
            if (_patrolPath == null || _patrolPath.Length == 0)
                return current;

            var target = _patrolPath[_patrolIndex];
            if ((target - current).sqrMagnitude <= arriveThreshold * arriveThreshold)
            {
                _patrolIndex = (_patrolIndex + 1) % _patrolPath.Length;
                target = _patrolPath[_patrolIndex];
            }

            return target;
        }

        private Vector2 ResolveWanderTarget(Vector2 home, float radius, Vector2 current, float arriveThreshold)
        {
            if (!_hasWanderTarget
                || (current - _wanderTarget).sqrMagnitude <= arriveThreshold * arriveThreshold)
            {
                _wanderTarget = PickRandomInRadius(home, radius);
                _hasWanderTarget = true;
            }

            return ClampToPitch(_wanderTarget);
        }

        private Vector2 PickRandomInRadius(Vector2 center, float radius)
        {
            if (_wanderRng == null)
                _wanderRng = new System.Random();

            radius = Mathf.Max(0.1f, radius);
            var angle = (float)_wanderRng.NextDouble() * Mathf.PI * 2f;
            var distance = Mathf.Sqrt((float)_wanderRng.NextDouble()) * radius;
            return ClampToPitch(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance);
        }

        private Vector2 ClampToPitch(Vector2 position) =>
            _pitchBounds != null ? _pitchBounds.Clamp(position) : position;

        private static Vector2 MoveTowards(
            Vector2 current,
            Vector2 target,
            float maxSpeed,
            float acceleration,
            float arriveThreshold,
            float deltaTime,
            ref Vector2 velocity)
        {
            var offset = target - current;
            if (offset.sqrMagnitude <= arriveThreshold * arriveThreshold)
            {
                velocity = Vector2.zero;
                return target;
            }

            var desiredVelocity = offset.normalized * maxSpeed;
            velocity = Vector2.MoveTowards(velocity, desiredVelocity, acceleration * deltaTime);
            var next = current + velocity * deltaTime;

            var remaining = target - next;
            if (Vector2.Dot(offset, remaining) <= 0f)
            {
                velocity = Vector2.zero;
                return target;
            }

            return next;
        }

        private static Vector2 ApplySeparation(
            Vector2 position,
            IReadOnlyList<Vector2> neighbors,
            float radius,
            float strength,
            float deltaTime)
        {
            if (radius <= 0f || neighbors == null || neighbors.Count == 0)
                return position;

            var push = Vector2.zero;
            for (var i = 0; i < neighbors.Count; i++)
            {
                var delta = position - neighbors[i];
                var dist = delta.magnitude;
                if (dist >= radius || dist < 0.0001f)
                    continue;

                push += delta.normalized * (1f - dist / radius);
            }

            if (push.sqrMagnitude < 0.0001f)
                return position;

            return position + push.normalized * (strength * 2f) * deltaTime;
        }
    }
}
