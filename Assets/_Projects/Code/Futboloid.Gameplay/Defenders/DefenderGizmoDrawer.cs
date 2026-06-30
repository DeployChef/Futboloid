#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Отрисовка gizmo в Scene view (editor-only Handles).</summary>
    public static class DefenderGizmoDrawer
    {
        public static void DrawLabel(Vector3 worldPosition, string text)
        {
#if UNITY_EDITOR
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            Handles.Label(worldPosition, text, style);
#endif
        }

        public static void DrawWireCircle(Vector3 center, float radius, Color color)
        {
            if (radius <= 0f)
                return;

            Gizmos.color = color;
            DrawCircleXY(center, radius, 32);
        }

        public static void DrawPatrolPath(Vector2[] points, Color color, bool closed)
        {
            if (points == null || points.Length < 2)
                return;

            Gizmos.color = color;
            for (var i = 0; i < points.Length; i++)
            {
                var a = new Vector3(points[i].x, points[i].y, 0f);
                var bIndex = closed ? (i + 1) % points.Length : i + 1;
                if (bIndex >= points.Length)
                    break;

                var b = new Vector3(points[bIndex].x, points[bIndex].y, 0f);
                Gizmos.DrawLine(a, b);
                Gizmos.DrawSphere(a, 0.06f);
            }
        }

        public static void DrawGoalkeeperHyperbola(
            Vector3 goalCenter,
            float halfWidth,
            float hyperbolaA,
            Color color,
            int segments = 24)
        {
            if (halfWidth <= 0f)
                return;

            Gizmos.color = color;
            var previous = Vector3.zero;
            for (var i = 0; i <= segments; i++)
            {
                var t = Mathf.Lerp(-1f, 1f, i / (float)segments);
                var x = goalCenter.x + t * halfWidth;
                var y = goalCenter.y + hyperbolaA * (1f - t * t);
                var point = new Vector3(x, y, goalCenter.z);

                if (i > 0)
                    Gizmos.DrawLine(previous, point);

                previous = point;
            }
        }

        private static void DrawCircleXY(Vector3 center, float radius, int segments)
        {
            var previous = center + new Vector3(radius, 0f, 0f);
            for (var i = 1; i <= segments; i++)
            {
                var angle = i / (float)segments * Mathf.PI * 2f;
                var next = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
