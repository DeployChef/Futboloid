using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    /// <summary>
    /// Прямоугольник игрового поля на сцене Game — clamp для акторов, страховка мяча при вылете.
    /// </summary>
    public class PitchBounds : MonoBehaviour
    {
        [SerializeField] private float minX = -4.5f;
        [SerializeField] private float maxX = 4.5f;
        [SerializeField] private float minY = -9.5f;
        [SerializeField] private float maxY = 8.5f;

        [Header("Player kickoff zone")]
        [SerializeField] private float kickoffMinX = -1.5f;
        [SerializeField] private float kickoffMaxX = 1.5f;

        [Header("Ball recovery")]
        [Tooltip("Если центр мяча дальше этой дистанции за границей поля — телепорт в центр.")]
        [SerializeField] private float ballRecoveryOverflow = 5f;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color playColor = new(0.2f, 0.9f, 0.3f, 0.85f);
        [SerializeField] private Color kickoffColor = new(0.3f, 0.75f, 1f, 0.7f);

        public float MinX => minX;
        public float MaxX => maxX;
        public float MinY => minY;
        public float MaxY => maxY;
        public float KickoffMinX => kickoffMinX;
        public float KickoffMaxX => kickoffMaxX;
        public float KickoffHalfWidth => (kickoffMaxX - kickoffMinX) * 0.5f;
        public float BallRecoveryOverflow => ballRecoveryOverflow;
        public Vector2 Center => new((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);

        public float DistanceOutside(Vector2 position)
        {
            var closest = new Vector2(
                Mathf.Clamp(position.x, minX, maxX),
                Mathf.Clamp(position.y, minY, maxY));
            return Vector2.Distance(position, closest);
        }

        public Vector2 Clamp(Vector2 position) =>
            new(
                Mathf.Clamp(position.x, minX, maxX),
                Mathf.Clamp(position.y, minY, maxY));

        public bool Contains(Vector2 position) =>
            position.x >= minX && position.x <= maxX
            && position.y >= minY && position.y <= maxY;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            DrawRects(selected: true);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            DrawRects(selected: false);
        }

        private void DrawRects(bool selected)
        {
            var play = playColor;
            var kickoff = kickoffColor;
            if (!selected)
            {
                play.a *= 0.45f;
                kickoff.a *= 0.45f;
            }

            DrawRect(minX, maxX, minY, maxY, play);
            DrawRect(kickoffMinX, kickoffMaxX, minY, Mathf.Min(maxY, minY + 2.5f), kickoff);
        }

        private static void DrawRect(float left, float right, float bottom, float top, Color color)
        {
            var a = new Vector3(left, bottom, 0f);
            var b = new Vector3(right, bottom, 0f);
            var c = new Vector3(right, top, 0f);
            var d = new Vector3(left, top, 0f);

            Gizmos.color = color;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
#endif
    }
}
