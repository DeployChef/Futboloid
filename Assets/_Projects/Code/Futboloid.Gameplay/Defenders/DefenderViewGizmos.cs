using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    public sealed class DefenderViewGizmos : MonoBehaviour
    {
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private float gizmoLabelHeight = 0.35f;
        [SerializeField] private float gizmoLabelPadding = 0.12f;
        [SerializeField] private DefenderView defender;
        [SerializeField] private DefenderHealth health;
        [SerializeField] private Collider2D bodyCollider;

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            DrawGizmos(selected: true);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            DrawGizmos(selected: false);
        }

        private void DrawGizmos(bool selected)
        {
            if (defender == null)
                return;

            var home = Application.isPlaying ? defender.HomePosition : (Vector2)transform.position;
            var center = new Vector3(home.x, home.y, transform.position.z);
            var labelPos = GetLabelPosition(center);

            var behavior = DefenderBehaviorMapping.GetShortLabel(defender.BehaviorKind);
            var label = $"#{defender.SlotId}  {defender.Role}\n{behavior} ({defender.HitType}+{defender.MovementType})";
            if (defender.RunningToGoal)
                label += "\n→ GK";
            if (defender.Role == DefenderRole.Field && !defender.RunningToGoal)
                label += $"\nMove: {defender.MovementType}";
            if (Application.isPlaying && health != null)
                label += $"\nHP: {health.CurrentHp}/{health.MaxHp}";

            DefenderGizmoDrawer.DrawLabel(labelPos, label);

            if (defender.Role == DefenderRole.Goalkeeper)
                return;

            var alpha = selected ? 0.85f : 0.4f;
            DefenderGizmoDrawer.DrawWireCircle(
                center,
                defender.SeparationRadius,
                new Color(1f, 0.55f, 0.1f, alpha * 0.7f));

            switch (defender.MovementType)
            {
                case DefenderMovementType.PatrolGenerated:
                    DefenderGizmoDrawer.DrawWireCircle(
                        center,
                        defender.PatrolRadius,
                        new Color(0.3f, 1f, 0.45f, alpha));
                    var path = PatrolPathGenerator.Generate(
                        home,
                        defender.PatrolPointCount,
                        defender.PatrolRadius,
                        defender.SlotId * 7919 + 17);
                    DefenderGizmoDrawer.DrawPatrolPath(
                        path,
                        new Color(1f, 0.92f, 0.2f, alpha),
                        closed: true);
                    break;

                case DefenderMovementType.WanderInRadius:
                case DefenderMovementType.ChaseBall:
                    DefenderGizmoDrawer.DrawWireCircle(
                        center,
                        defender.WanderRadius,
                        defender.MovementType == DefenderMovementType.ChaseBall
                            ? new Color(0.2f, 0.95f, 1f, alpha)
                            : new Color(0.3f, 0.75f, 1f, alpha));
                    break;
            }
        }

        private Vector3 GetLabelPosition(Vector3 center)
        {
            if (bodyCollider != null)
                return new Vector3(center.x, bodyCollider.bounds.max.y + gizmoLabelPadding, center.z);

            return center + Vector3.up * gizmoLabelHeight;
        }
    }
}
