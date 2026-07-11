using System.Collections.Generic;
using Futboloid.Core.StatusEffects;
using Futboloid.Gameplay.Match;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Tribune
{
    /// <summary>
    /// Спавнит предметы с вертикальных линий по бокам поля по дуге во время Simulating.
    /// Настройки и gizmo — на этом компоненте. На том же объекте тикает <see cref="IStatusEffectService"/>.
    /// </summary>
    public class TribuneSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TribuneItemView itemPrefab;
        [SerializeField] private Transform itemRoot;
        [SerializeField] private PitchBounds pitchBounds;
        [SerializeField] private List<StatusEffectDefinition> spawnPool = new();

        [Header("Spawn lines")]
        [Tooltip("Расстояние от центра поля до вертикальной линии спавна (симметрично L/R).")]
        [SerializeField] private float spawnLineOffset = 6f;
        [SerializeField] private float spawnYMin = 4f;
        [SerializeField] private float spawnYMax = 7.5f;

        [Header("Target zone")]
        [SerializeField] private float targetMinX = -2f;
        [SerializeField] private float targetMaxX = 2f;
        [SerializeField] private float targetMinY = -7.5f;
        [SerializeField] private float targetMaxY = -6f;

        [Header("Arc")]
        [SerializeField] private float arcHeightMin = 3f;
        [SerializeField] private float arcHeightMax = 6f;
        [SerializeField] private float overshootPastTarget = 3.5f;

        [Header("Timing")]
        [SerializeField] private float spawnIntervalSeconds = 4f;
        [SerializeField] private float spawnIntervalJitter = 1f;
        [SerializeField] private float minSpawnIntervalSeconds = 2f;
        [SerializeField] private float flightDurationMin = 5f;
        [SerializeField] private float flightDurationMax = 8f;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color spawnLineColor = new(1f, 0.75f, 0.2f, 0.9f);
        [SerializeField] private Color targetZoneColor = new(0.3f, 0.9f, 0.45f, 0.85f);
        [SerializeField] private Color minArcColor = new(0.4f, 0.85f, 1f, 0.9f);
        [SerializeField] private Color maxArcColor = new(1f, 0.45f, 0.35f, 0.9f);

        private PitchStateMachine _pitch;
        private PitchBounds _bounds;
        private IStatusEffectService _statusEffects;
        private float _spawnTimer;

        [Inject]
        public void Construct(
            PitchStateMachine pitch,
            PitchBounds bounds,
            IStatusEffectService statusEffects)
        {
            _pitch = pitch;
            _bounds = pitchBounds != null ? pitchBounds : bounds;
            _statusEffects = statusEffects;
            ResetSpawnTimer();
        }

        private void Update()
        {
            if (_pitch == null || ResolveBounds() == null || _statusEffects == null)
                return;

            if (_pitch.IsSimulating)
            {
                _statusEffects.Tick(Time.deltaTime);
                TickSpawn(Time.deltaTime);
            }
        }

        private void TickSpawn(float deltaTime)
        {
            if (itemPrefab == null || spawnPool.Count == 0)
                return;

            _spawnTimer -= deltaTime;
            if (_spawnTimer > 0f)
                return;

            SpawnRandomItem();
            ResetSpawnTimer();
        }

        private void SpawnRandomItem()
        {
            var definition = spawnPool[Random.Range(0, spawnPool.Count)];
            if (definition == null)
                return;

            var bounds = ResolveBounds();
            var centerX = bounds.Center.x;
            var fromLeft = Random.value < 0.5f;
            var lineX = fromLeft ? centerX - spawnLineOffset : centerX + spawnLineOffset;
            var spawnY = Random.Range(spawnYMin, spawnYMax);
            var start = new Vector2(lineX, spawnY);

            var aim = new Vector2(
                Random.Range(targetMinX, targetMaxX),
                Random.Range(targetMinY, targetMaxY));

            var flyDirection = aim - start;
            if (flyDirection.sqrMagnitude < 0.01f)
                flyDirection = Vector2.down;
            else
                flyDirection.Normalize();

            var end = aim + flyDirection * overshootPastTarget;
            var arcHeight = Random.Range(
                Mathf.Min(arcHeightMin, arcHeightMax),
                Mathf.Max(arcHeightMin, arcHeightMax));
            var duration = Random.Range(
                Mathf.Min(flightDurationMin, flightDurationMax),
                Mathf.Max(flightDurationMin, flightDurationMax));

            var parent = itemRoot != null ? itemRoot : transform;
            var item = Instantiate(
                itemPrefab,
                new Vector3(start.x, start.y, parent.position.z),
                Quaternion.identity,
                parent);
            item.Initialize(
                definition,
                _statusEffects,
                start,
                end,
                arcHeight,
                duration);
        }

        private void ResetSpawnTimer()
        {
            var jitter = Random.Range(-spawnIntervalJitter, spawnIntervalJitter);
            var interval = spawnIntervalSeconds + jitter;
            _spawnTimer = Mathf.Max(minSpawnIntervalSeconds, interval);
        }

        private PitchBounds ResolveBounds() => pitchBounds != null ? pitchBounds : _bounds;

        private float ResolveCenterX()
        {
            var bounds = ResolveBounds();
            return bounds != null ? bounds.Center.x : 0f;
        }

        private static Vector2 ComputeControl(Vector2 start, Vector2 end, float arcHeight) =>
            new((start.x + end.x) * 0.5f, start.y + arcHeight);

        private static Vector2 EvaluateBezier(Vector2 start, Vector2 control, Vector2 end, float t)
        {
            var u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() => DrawGizmos(selected: true);

        private void OnDrawGizmos() => DrawGizmos(selected: false);

        private void DrawGizmos(bool selected)
        {
            if (!drawGizmos)
                return;

            var centerX = ResolveCenterX();
            var leftX = centerX - spawnLineOffset;
            var rightX = centerX + spawnLineOffset;

            var spawnColor = spawnLineColor;
            var targetColor = targetZoneColor;
            var minColor = minArcColor;
            var maxColor = maxArcColor;
            if (!selected)
            {
                spawnColor.a *= 0.45f;
                targetColor.a *= 0.45f;
                minColor.a *= 0.45f;
                maxColor.a *= 0.45f;
            }

            DrawVerticalLine(leftX, spawnYMin, spawnYMax, spawnColor);
            DrawVerticalLine(rightX, spawnYMin, spawnYMax, spawnColor);
            DrawTargetZone(targetColor);

            var minArc = Mathf.Min(arcHeightMin, arcHeightMax);
            var maxArc = Mathf.Max(arcHeightMin, arcHeightMax);
            var targetBottomLeft = new Vector2(targetMinX, targetMinY);
            var targetTopRight = new Vector2(targetMaxX, targetMaxY);

            DrawBezierArc(
                new Vector2(leftX, spawnYMin),
                targetBottomLeft,
                minArc,
                minColor);
            DrawBezierArc(
                new Vector2(leftX, spawnYMax),
                targetTopRight,
                maxArc,
                maxColor);
            DrawBezierArc(
                new Vector2(rightX, spawnYMin),
                targetBottomLeft,
                minArc,
                minColor);
            DrawBezierArc(
                new Vector2(rightX, spawnYMax),
                targetTopRight,
                maxArc,
                maxColor);
        }

        private void DrawTargetZone(Color color)
        {
            var left = Mathf.Min(targetMinX, targetMaxX);
            var right = Mathf.Max(targetMinX, targetMaxX);
            var bottom = Mathf.Min(targetMinY, targetMaxY);
            var top = Mathf.Max(targetMinY, targetMaxY);

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

        private static void DrawVerticalLine(float x, float yMin, float yMax, Color color)
        {
            var bottom = Mathf.Min(yMin, yMax);
            var top = Mathf.Max(yMin, yMax);

            Gizmos.color = color;
            Gizmos.DrawLine(new Vector3(x, bottom, 0f), new Vector3(x, top, 0f));
        }

        private static void DrawBezierArc(Vector2 start, Vector2 end, float arcHeight, Color color, int segments = 28)
        {
            var control = ComputeControl(start, end, arcHeight);
            Gizmos.color = color;

            var previous = start;
            for (var i = 1; i <= segments; i++)
            {
                var t = i / (float)segments;
                var point = EvaluateBezier(start, control, end, t);
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
        }
#endif
    }
}
