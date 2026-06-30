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

        public static void DrawGoalkeeperParabola(
            Vector3 goalCenter,
            float halfWidth,
            float parabolaHeight,
            Color trajectoryColor,
            bool selected = false,
            int segments = 32)
        {
            if (halfWidth <= 0f)
                return;

            var z = goalCenter.z;
            var cx = goalCenter.x;
            var goalY = goalCenter.y;
            var left = new Vector3(cx - halfWidth, goalY, z);
            var right = new Vector3(cx + halfWidth, goalY, z);
            var centerOnLine = new Vector3(cx, goalY, z);
            var vertex = new Vector3(cx, goalY - parabolaHeight, z);

            var lineAlpha = selected ? 0.95f : 0.4f;
            var goalLineColor = new Color(0.9f, 0.9f, 0.9f, lineAlpha);
            var postColor = new Color(1f, 0.82f, 0.15f, lineAlpha);
            var widthColor = new Color(0.65f, 0.65f, 0.65f, lineAlpha * 0.55f);
            var heightColor = new Color(0.35f, 0.85f, 1f, lineAlpha * 0.75f);
            var markerSize = selected ? 0.07f : 0.045f;
            var postHeight = Mathf.Max(0.12f, parabolaHeight * 0.2f);

            DrawDottedLine(left, right, goalLineColor, selected ? 5f : 7f);

            Gizmos.color = postColor;
            Gizmos.DrawLine(left, left + Vector3.up * postHeight);
            Gizmos.DrawLine(right, right + Vector3.up * postHeight);

            Gizmos.color = widthColor;
            Gizmos.DrawLine(centerOnLine, left);
            Gizmos.DrawLine(centerOnLine, right);

            DrawDottedLine(centerOnLine, vertex, heightColor, 3f);

            Gizmos.color = trajectoryColor;
            var previous = SampleParabola(cx, goalY, halfWidth, parabolaHeight, z, -1f);
            for (var i = 1; i <= segments; i++)
            {
                var t = Mathf.Lerp(-1f, 1f, i / (float)segments);
                var point = SampleParabola(cx, goalY, halfWidth, parabolaHeight, z, t);
                Gizmos.DrawLine(previous, point);
                previous = point;
            }

            Gizmos.color = postColor;
            Gizmos.DrawSphere(left, markerSize);
            Gizmos.DrawSphere(right, markerSize);

            Gizmos.color = trajectoryColor;
            Gizmos.DrawSphere(vertex, markerSize * 1.15f);

            DrawParabolaDirectionArrow(cx, goalY, halfWidth, parabolaHeight, z, 0.2f, trajectoryColor, selected);

            if (selected)
            {
                DrawMiniLabel(vertex + Vector3.down * 0.1f, $"парабола  h {parabolaHeight:0.##}");
                DrawMiniLabel(new Vector3(cx, goalY + 0.12f, z), $"w {halfWidth * 2f:0.##}");
                DrawMiniLabel(left + Vector3.left * 0.08f, "t=-1");
                DrawMiniLabel(right + Vector3.right * 0.08f, "t=1");
            }
        }

        private static Vector3 SampleParabola(
            float centerX,
            float goalY,
            float halfWidth,
            float parabolaHeight,
            float z,
            float t)
        {
            var x = centerX + t * halfWidth;
            var y = goalY - parabolaHeight * (1f - t * t);
            return new Vector3(x, y, z);
        }

        private static void DrawParabolaDirectionArrow(
            float centerX,
            float goalY,
            float halfWidth,
            float parabolaHeight,
            float z,
            float t,
            Color color,
            bool selected)
        {
            var point = SampleParabola(centerX, goalY, halfWidth, parabolaHeight, z, t);
            var tangent = new Vector2(halfWidth, 2f * parabolaHeight * t);
            if (tangent.sqrMagnitude < 0.0001f)
                return;

            tangent.Normalize();
            var arrowLen = selected ? 0.22f : 0.16f;
            var tip = point + new Vector3(tangent.x, tangent.y, 0f) * arrowLen;
            var side = new Vector3(-tangent.y, tangent.x, 0f) * arrowLen * 0.35f;

            Gizmos.color = color;
            Gizmos.DrawLine(point, tip);
            Gizmos.DrawLine(tip, tip - new Vector3(tangent.x, tangent.y, 0f) * arrowLen * 0.45f + side);
            Gizmos.DrawLine(tip, tip - new Vector3(tangent.x, tangent.y, 0f) * arrowLen * 0.45f - side);
        }

        private static void DrawDottedLine(Vector3 from, Vector3 to, Color color, float screenSpaceSize)
        {
#if UNITY_EDITOR
            Handles.color = color;
            Handles.DrawDottedLine(from, to, screenSpaceSize);
#else
            Gizmos.color = color;
            Gizmos.DrawLine(from, to);
#endif
        }

        private static void DrawMiniLabel(Vector3 position, string text)
        {
#if UNITY_EDITOR
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.95f, 0.95f, 0.9f) }
            };
            Handles.Label(position, text, style);
#endif
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
