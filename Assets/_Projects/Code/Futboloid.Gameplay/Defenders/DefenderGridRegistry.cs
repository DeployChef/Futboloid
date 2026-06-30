using System;
using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Match;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Реестр футболистов на сцене + точка конфигурации зоны ворот для promotion.</summary>
    public class DefenderGridRegistry : MonoBehaviour
    {
        [Header("Goalkeeper promotion (read by DefenderPromotionService)")]
        [SerializeField] private Transform goalAnchor;
        [SerializeField] private float runToGoalSpeed = 4f;
        [SerializeField] private float runToGoalAcceleration = 18f;
        [SerializeField] private float arriveThreshold = 0.08f;

        [Header("Reshuffle after goal")]
        [SerializeField] private float reshuffleSpeed = 3.5f;
        [SerializeField] private float reshuffleAcceleration = 14f;

        private readonly Dictionary<EntityId, DefenderView> _byColliderId = new();
        private readonly List<DefenderView> _defenders = new();

        private IGameEventBus _bus;
        private MatchFlow _matchFlow;

        public float RunToGoalSpeed => runToGoalSpeed;
        public float RunToGoalAcceleration => runToGoalAcceleration;
        public float ReshuffleSpeed => reshuffleSpeed;
        public float ReshuffleAcceleration => reshuffleAcceleration;
        public float ArriveThreshold => arriveThreshold;

        [Inject]
        public void Construct(IGameEventBus bus, MatchFlow matchFlow)
        {
            _bus = bus;
            _matchFlow = matchFlow;
            _bus.Subscribe<DefenderDestroyedEvent>(OnDefenderDestroyed);
        }

        public void Register(DefenderView defender)
        {
            if (defender == null || _defenders.Contains(defender))
                return;

            _defenders.Add(defender);
            RegisterCollider(defender.ContactCollider, defender);
        }

        public void Unregister(DefenderView defender)
        {
            if (defender == null)
                return;

            UnregisterCollider(defender.ContactCollider);
            _defenders.Remove(defender);
        }

        public bool TryGetDefender(Collider2D contactCollider, out DefenderView defender)
        {
            defender = null;
            if (contactCollider == null)
                return false;

            return _byColliderId.TryGetValue(contactCollider.GetEntityId(), out defender);
        }

        private void RegisterCollider(Collider2D contactCollider, DefenderView defender)
        {
            if (contactCollider == null || defender == null)
                return;

            _byColliderId[contactCollider.GetEntityId()] = defender;
        }

        private void UnregisterCollider(Collider2D contactCollider)
        {
            if (contactCollider == null)
                return;

            _byColliderId.Remove(contactCollider.GetEntityId());
        }

        public int AliveCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _defenders.Count; i++)
                {
                    if (_defenders[i] != null && _defenders[i].IsAlive)
                        count++;
                }

                return count;
            }
        }

        public bool HasLivingGoalkeeper()
        {
            for (var i = 0; i < _defenders.Count; i++)
            {
                var defender = _defenders[i];
                if (defender != null && defender.IsAlive && defender.Role == DefenderRole.Goalkeeper)
                    return true;
            }

            return false;
        }

        public DefenderView FindNearestLivingField(Vector2 point, bool excludeRunning = false)
        {
            DefenderView best = null;
            var bestDist = float.MaxValue;

            for (var i = 0; i < _defenders.Count; i++)
            {
                var defender = _defenders[i];
                if (defender == null || !defender.IsAlive || defender.Role != DefenderRole.Field)
                    continue;

                if (excludeRunning && defender.RunningToGoal)
                    continue;

                var dist = ((Vector2)defender.transform.position - point).sqrMagnitude;
                if (dist >= bestDist)
                    continue;

                bestDist = dist;
                best = defender;
            }

            return best;
        }

        public void ForEachLiving(Action<DefenderView> action)
        {
            if (action == null)
                return;

            for (var i = 0; i < _defenders.Count; i++)
            {
                var defender = _defenders[i];
                if (defender != null && defender.IsAlive)
                    action(defender);
            }
        }

        public Transform ResolveGoalAnchor()
        {
            if (goalAnchor != null)
                return goalAnchor;

            var zone = FindAnyObjectByType<GoalAnchor>();
            return zone != null ? zone.transform : null;
        }

        private void OnDefenderDestroyed(DefenderDestroyedEvent _)
        {
            if (_matchFlow == null || AliveCount > 0)
                return;

            _matchFlow.EndMatchFromWipe();
        }
    }
}
