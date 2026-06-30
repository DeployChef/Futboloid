using System;
using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Match;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Замена вратаря: scope-сервис, не MonoBehaviour. Создаётся в GameScope.</summary>
    public sealed class DefenderPromotionService : IDisposable
    {
        private readonly IGameEventBus _bus;
        private readonly DefenderGridRegistry _registry;
        private readonly List<IDisposable> _subscriptions = new();

        private DefenderView _activeCandidate;

        public DefenderPromotionService(IGameEventBus bus, DefenderGridRegistry registry)
        {
            _bus = bus;
            _registry = registry;
            _subscriptions.Add(bus.Subscribe<DefenderDestroyedEvent>(OnDefenderDestroyed));
            _subscriptions.Add(bus.Subscribe<DefenderPromotionCompletedEvent>(OnPromotionCompleted));
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnDefenderDestroyed(DefenderDestroyedEvent e)
        {
            if (_activeCandidate != null && _activeCandidate.SlotId == e.SlotId)
                _activeCandidate = null;

            if (_registry == null || _registry.HasLivingGoalkeeper())
                return;

            TryStartPromotion();
        }

        private void OnPromotionCompleted(DefenderPromotionCompletedEvent _)
        {
            _activeCandidate = null;
        }

        private void TryStartPromotion()
        {
            if (_activeCandidate != null && _activeCandidate.IsAlive && _activeCandidate.RunningToGoal)
                return;

            _activeCandidate = null;

            var anchor = _registry.ResolveGoalAnchor();
            if (anchor == null)
            {
                Debug.LogWarning("[DefenderPromotionService] Goal anchor not found — cannot promote field player.");
                return;
            }

            var candidate = _registry.FindNearestLivingField(anchor.position, excludeRunning: true);
            if (candidate == null)
            {
                Debug.Log("[DefenderPromotionService] No living field player to replace goalkeeper.");
                return;
            }

            _activeCandidate = candidate;
            candidate.BeginRunToGoal(anchor, _registry.RunToGoalSpeed, _registry.ArriveThreshold);
            _bus.Publish(new DefenderPromotionStartedEvent(candidate.SlotId));
            Debug.Log($"[DefenderPromotionService] Slot {candidate.SlotId} running to goal.");
        }
    }
}
