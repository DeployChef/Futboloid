using UnityEngine;
using UnityEngine.Serialization;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Точка зоны ворот соперника. Движение GK — парабола вглубь поля:
    /// y = goalLineY − height·(1 − t²), x = centerX + t·halfWidth.
    /// </summary>
    public class GoalAnchor : MonoBehaviour
    {
        [SerializeField] private float halfWidth = 2f;
        [FormerlySerializedAs("hyperbolaA")]
        [Tooltip("Насколько GK выходит к полю в центре ворот (вниз по Y).")]
        [SerializeField] private float parabolaHeight = 0.35f;

        public Vector2 Center => transform.position;
        public float HalfWidth => halfWidth;
        public float GoalLineY => transform.position.y;
        public float ParabolaHeight => parabolaHeight;

        public Vector2 PositionOnParabola(float t)
        {
            var x = Center.x + t * halfWidth;
            var y = GoalLineY - parabolaHeight * (1f - t * t);
            return new Vector2(x, y);
        }

        public float ParamFromWorldX(float worldX)
        {
            if (halfWidth <= 0f)
                return 0f;

            return Mathf.Clamp((worldX - Center.x) / halfWidth, -1f, 1f);
        }

        private void OnDrawGizmos()
        {
            DefenderGizmoDrawer.DrawGoalkeeperParabola(
                new Vector3(Center.x, GoalLineY, transform.position.z),
                halfWidth,
                parabolaHeight,
                new Color(1f, 0.45f, 1f, 0.35f),
                selected: false);
        }

        private void OnDrawGizmosSelected()
        {
            DefenderGizmoDrawer.DrawGoalkeeperParabola(
                new Vector3(Center.x, GoalLineY, transform.position.z),
                halfWidth,
                parabolaHeight,
                new Color(1f, 0.45f, 1f, 0.95f),
                selected: true);
        }
    }
}
