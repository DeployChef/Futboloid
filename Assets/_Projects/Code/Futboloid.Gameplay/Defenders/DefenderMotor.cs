using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    public class DefenderMotor
    {
        private float _param;

        public float Param => _param;

        public Vector2 TickRunTowards(
            Vector2 current,
            Vector2 target,
            float speed,
            float deltaTime,
            out bool arrived)
        {
            var next = Vector2.MoveTowards(current, target, speed * deltaTime);
            arrived = (target - next).sqrMagnitude < 0.0001f;
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
