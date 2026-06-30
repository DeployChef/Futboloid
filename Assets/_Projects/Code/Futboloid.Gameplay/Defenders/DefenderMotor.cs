using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    public class DefenderMotor
    {
        private float _param;
        private Vector2 _runVelocity;

        public float Param => _param;

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

        /// <summary>Следует за ballWorldX: t → clamp((x−center)/halfWidth), движется по параболе.</summary>
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
            _param = Mathf.MoveTowards(_param, targetT, speed * deltaTime);
            return zone.PositionOnParabola(_param);
        }

        public void ResetGoalkeeperParam(float startParam)
        {
            _param = Mathf.Clamp(startParam, -1f, 1f);
        }
    }
}
