using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    public static class PatrolPathGenerator
    {
        public static Vector2[] Generate(Vector2 center, int pointCount, float radius, int seed)
        {
            pointCount = Mathf.Max(2, pointCount);
            radius = Mathf.Max(0.1f, radius);

            var points = new Vector2[pointCount];
            var rng = new System.Random(seed);

            for (var i = 0; i < pointCount; i++)
            {
                var angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                var distance = Mathf.Sqrt((float)rng.NextDouble()) * radius;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            }

            return points;
        }
    }
}
