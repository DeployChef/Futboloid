using System.Collections.Generic;
using Futboloid.Core.StatusEffects;
using Futboloid.Gameplay.Match;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Tribune
{
    /// <summary>
    /// Спавнит предметы с боков поля по дуге во время Simulating.
    /// На том же объекте тикает <see cref="IStatusEffectService"/>.
    /// </summary>
    public class TribuneSpawner : MonoBehaviour
    {
        [SerializeField] private TribuneItemView itemPrefab;
        [SerializeField] private Transform itemRoot;
        [SerializeField] private List<StatusEffectDefinition> spawnPool = new();

        private PitchStateMachine _pitch;
        private PitchBounds _bounds;
        private IStatusEffectService _statusEffects;
        private TribuneSpawnSettings _settings;
        private float _spawnTimer;

        [Inject]
        public void Construct(
            PitchStateMachine pitch,
            PitchBounds bounds,
            IStatusEffectService statusEffects,
            TribuneSpawnSettings settings)
        {
            _pitch = pitch;
            _bounds = bounds;
            _statusEffects = statusEffects;
            _settings = settings;
            ResetSpawnTimer();
        }

        private void Update()
        {
            if (_pitch == null || _bounds == null || _statusEffects == null || _settings == null)
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

            var fromLeft = Random.value < 0.5f;
            var startX = fromLeft
                ? _bounds.MinX - _settings.HorizontalSpawnOffset
                : _bounds.MaxX + _settings.HorizontalSpawnOffset;

            var start = new Vector2(
                startX,
                Random.Range(_settings.SpawnMinY, _settings.SpawnMaxY));

            var end = new Vector2(
                Random.Range(_bounds.MinX, _bounds.MaxX),
                Random.Range(_settings.TargetMinY, _settings.TargetMaxY));

            var parent = itemRoot != null ? itemRoot : transform;
            var item = Instantiate(itemPrefab, parent);
            item.Initialize(
                definition,
                _statusEffects,
                start,
                end,
                _settings.ArcHeight,
                _settings.FlightDurationSeconds);
        }

        private void ResetSpawnTimer()
        {
            var jitter = _settings != null
                ? Random.Range(-_settings.SpawnIntervalJitter, _settings.SpawnIntervalJitter)
                : 0f;

            var interval = (_settings?.SpawnIntervalSeconds ?? 10f) + jitter;
            _spawnTimer = Mathf.Max(1f, interval);
        }
    }
}
