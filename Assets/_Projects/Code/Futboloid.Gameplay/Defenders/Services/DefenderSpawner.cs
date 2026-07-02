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
        [SerializeField] private int goalkeeperSlotId = DefenderFormationPatterns.GoalkeeperSlotId;

        private IObjectResolver _resolver;
        private ITournamentBracketReadModel _tournament;
        private readonly List<DefenderView> _spawned = new();
        private readonly List<int> _fieldSlotBuffer = new();
        private readonly List<IDisposable> _subscriptions = new();

        [Inject]
        public void Construct(
            IGameEventBus bus,
            IObjectResolver resolver,
            ITournamentBracketReadModel tournament)
        {
            _resolver = resolver;
            _tournament = tournament;
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

            ClearSpawned();

            var matchNumber = _tournament?.CurrentMatchNumber ?? 1;
            DefenderFormationPatterns.CollectFieldSlots(matchNumber, _fieldSlotBuffer);

            SpawnGoalkeeper();
            SpawnFieldDefenders(_fieldSlotBuffer);
        }

        private void SpawnGoalkeeper()
        {
            if (goalAnchor == null)
            {
                Debug.LogWarning("[DefenderSpawner] Goal anchor is not assigned.", this);
                return;
            }

            var zone = goalAnchor.GetComponent<GoalAnchor>();
            var home = zone != null ? zone.PositionOnParabola(0f) : (Vector2)goalAnchor.position;
            SpawnDefender(goalkeeperSlotId, DefenderRole.Goalkeeper, home, goalAnchor);
        }

        private void SpawnFieldDefenders(IReadOnlyList<int> slotIds)
        {
            if (slotLayout == null)
            {
                Debug.LogWarning("[DefenderSpawner] DefenderSlotLayout is not assigned.", this);
                return;
            }

            for (var i = 0; i < slotIds.Count; i++)
            {
                var slotId = slotIds[i];
                if (!slotLayout.TryGetPosition(slotId, out var home))
                {
                    Debug.LogWarning($"[DefenderSpawner] Slot #{slotId} has no position in layout.", this);
                    continue;
                }

                SpawnDefender(slotId, DefenderRole.Field, home, goalAnchor);
            }
        }

        private void SpawnDefender(int slotId, DefenderRole role, Vector2 home, Transform anchor)
        {
            var parent = spawnRoot != null ? spawnRoot : transform;
            var instance = Instantiate(
                defenderPrefab,
                new Vector3(home.x, home.y, parent.position.z),
                Quaternion.identity,
                parent);

            instance.ApplySpawnSetup(slotId, role, home, anchor);
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
