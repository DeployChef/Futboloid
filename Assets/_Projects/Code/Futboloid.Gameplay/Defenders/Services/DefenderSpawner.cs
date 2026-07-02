using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Генерирует врагов на слотах при старте / сбросе матча.</summary>
    public sealed class DefenderSpawner : MonoBehaviour
    {
        [SerializeField] private DefenderView defenderPrefab;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private DefenderSlotLayout slotLayout;
        [SerializeField] private Transform goalAnchor;
        [SerializeField] private DefenderGenerationSettings generationSettingsOverride;

        private IObjectResolver _resolver;
        private ITournamentBracketReadModel _tournament;
        private DefenderGenerationSettings _generationSettings;
        private readonly List<DefenderView> _spawned = new();
        private readonly List<IDisposable> _subscriptions = new();

        [Inject]
        public void Construct(
            IGameEventBus bus,
            IObjectResolver resolver,
            ITournamentBracketReadModel tournament,
            DefenderGenerationSettings generationSettings)
        {
            _resolver = resolver;
            _tournament = tournament;
            _generationSettings = generationSettingsOverride != null
                ? generationSettingsOverride
                : generationSettings;
            _subscriptions.Add(bus.Subscribe<PitchResetRequestedEvent>(_ => SpawnForCurrentMatch()));
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void SpawnForCurrentMatch()
        {
            if (defenderPrefab == null)
            {
                Debug.LogWarning("[DefenderSpawner] Defender prefab is not assigned.", this);
                return;
            }

            if (_generationSettings == null)
            {
                Debug.LogWarning("[DefenderSpawner] DefenderGenerationSettings is not available.", this);
                return;
            }

            ClearSpawned();

            var matchNumber = _tournament?.CurrentMatchNumber ?? 1;
            var context = new DefenderGenerationContext(matchNumber);
            var generation = DefenderMatchGenerator.Generate(_generationSettings, context);

            SpawnGoalkeeper(generation.Goalkeeper);
            SpawnFieldDefenders(generation.Field);
        }

        private void SpawnGoalkeeper(in DefenderBuild build)
        {
            if (goalAnchor == null)
            {
                Debug.LogWarning("[DefenderSpawner] Goal anchor is not assigned.", this);
                return;
            }

            var zone = goalAnchor.GetComponent<GoalAnchor>();
            var home = zone != null ? zone.PositionOnParabola(0f) : (Vector2)goalAnchor.position;
            SpawnDefender(build, home, goalAnchor);
        }

        private void SpawnFieldDefenders(IReadOnlyList<DefenderBuild> builds)
        {
            if (slotLayout == null)
            {
                Debug.LogWarning("[DefenderSpawner] DefenderSlotLayout is not assigned.", this);
                return;
            }

            for (var i = 0; i < builds.Count; i++)
            {
                var build = builds[i];
                if (!slotLayout.TryGetPosition(build.SlotId, out var home))
                {
                    Debug.LogWarning(
                        $"[DefenderSpawner] Slot #{build.SlotId} has no position in layout.",
                        this);
                    continue;
                }

                SpawnDefender(build, home, goalAnchor);
            }
        }

        private void SpawnDefender(in DefenderBuild build, Vector2 home, Transform anchor)
        {
            var parent = spawnRoot != null ? spawnRoot : transform;
            var instance = Instantiate(
                defenderPrefab,
                new Vector3(home.x, home.y, parent.position.z),
                Quaternion.identity,
                parent);

            instance.ApplySpawnSetup(build, home, anchor);
            _resolver.InjectGameObject(instance.gameObject);
            _spawned.Add(instance);
        }

        private void ClearSpawned()
        {
            for (var i = _spawned.Count - 1; i >= 0; i--)
            {
                var defender = _spawned[i];
                if (defender != null)
                    Destroy(defender.gameObject);
            }

            _spawned.Clear();

            if (spawnRoot == null)
                return;

            for (var i = spawnRoot.childCount - 1; i >= 0; i--)
            {
                var child = spawnRoot.GetChild(i);
                if (child.GetComponent<DefenderView>() != null)
                    Destroy(child.gameObject);
            }
        }
    }
}
