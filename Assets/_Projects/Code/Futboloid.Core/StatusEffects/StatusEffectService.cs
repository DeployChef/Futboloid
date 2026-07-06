using System;
using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;

namespace Futboloid.Core.StatusEffects
{
    public sealed class StatusEffectService : IStatusEffectService, IDisposable
    {
        private sealed class ActiveEffect
        {
            public int InstanceId;
            public StatusEffectDefinition Definition;
            public float RemainingSeconds;
            public float TotalDuration;
        }

        private readonly IGameEventBus _bus;
        private readonly List<ActiveEffect> _active = new();
        private readonly List<ActiveEffectSnapshot> _snapshotBuffer = new();
        private readonly List<int> _expiredInstanceIds = new();

        private int _nextInstanceId = 1;

        private readonly IDisposable _matchEndedSubscription;
        private readonly IDisposable _pitchResetSubscription;

        public StatusEffectService(IGameEventBus bus)
        {
            _bus = bus;
            _matchEndedSubscription = bus.Subscribe<MatchEndedEvent>(_ => ClearAll(StatusEffectRemoveReason.MatchEnd));
            _pitchResetSubscription = bus.Subscribe<PitchResetRequestedEvent>(_ => ClearAll(StatusEffectRemoveReason.MatchEnd));
        }

        public void Dispose()
        {
            _matchEndedSubscription?.Dispose();
            _pitchResetSubscription?.Dispose();
        }

        public void Apply(StatusEffectDefinition definition, int stacks = 1)
        {
            if (definition == null || string.IsNullOrEmpty(definition.Id))
                return;

            _ = stacks;

            var existing = FindByEffectId(definition.Id);
            if (existing != null)
            {
                existing.RemainingSeconds = definition.DurationSeconds;
                existing.TotalDuration = definition.DurationSeconds;

                _bus.Publish(new StatusEffectRefreshedEvent(existing.InstanceId, definition.DurationSeconds));
                Debug.Log($"[StatusEffectService] Refreshed {definition.Id} for {definition.DurationSeconds:0.#}s.");
                return;
            }

            var duration = definition.DurationSeconds;
            var instance = new ActiveEffect
            {
                InstanceId = _nextInstanceId++,
                Definition = definition,
                RemainingSeconds = duration,
                TotalDuration = duration,
            };

            _active.Add(instance);

            _bus.Publish(new StatusEffectAppliedEvent(
                instance.InstanceId,
                definition.Id,
                definition.DisplayName,
                definition.Description,
                definition.Icon,
                definition.IsDebuff,
                duration,
                charges: 0));

            Debug.Log($"[StatusEffectService] Applied {definition.Id} for {duration:0.#}s.");
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || _active.Count == 0)
                return;

            _expiredInstanceIds.Clear();

            for (var i = 0; i < _active.Count; i++)
            {
                var effect = _active[i];
                if (effect.TotalDuration <= 0f)
                    continue;

                effect.RemainingSeconds -= deltaTime;
                if (effect.RemainingSeconds <= 0f)
                    _expiredInstanceIds.Add(effect.InstanceId);
            }

            for (var i = 0; i < _expiredInstanceIds.Count; i++)
                RemoveInstance(_expiredInstanceIds[i], StatusEffectRemoveReason.Expired);
        }

        public float GetMultiplier(StatId stat)
        {
            var product = 1f;

            for (var i = 0; i < _active.Count; i++)
            {
                var definition = _active[i].Definition;
                if (definition != null && definition.AffectedStat == stat)
                    product *= definition.Multiplier;
            }

            return product;
        }

        public float GetAdditive(StatId stat)
        {
            _ = stat;
            return 0f;
        }

        public IReadOnlyList<ActiveEffectSnapshot> GetActiveForHud()
        {
            _snapshotBuffer.Clear();

            for (var i = 0; i < _active.Count; i++)
            {
                var effect = _active[i];
                var definition = effect.Definition;
                if (definition == null)
                    continue;

                var duration01 = effect.TotalDuration > 0f
                    ? Mathf.Clamp01(effect.RemainingSeconds / effect.TotalDuration)
                    : 1f;

                _snapshotBuffer.Add(new ActiveEffectSnapshot(
                    definition.Id,
                    definition.Icon,
                    duration01,
                    effect.RemainingSeconds,
                    charges: 0,
                    showTimerRing: effect.TotalDuration > 0f));
            }

            return _snapshotBuffer;
        }

        public void ClearAll(StatusEffectRemoveReason reason)
        {
            if (_active.Count == 0)
                return;

            var instanceIds = new int[_active.Count];
            for (var i = 0; i < _active.Count; i++)
                instanceIds[i] = _active[i].InstanceId;

            _active.Clear();

            for (var i = 0; i < instanceIds.Length; i++)
                _bus.Publish(new StatusEffectRemovedEvent(instanceIds[i], reason));
        }

        private ActiveEffect FindByEffectId(string effectId)
        {
            for (var i = 0; i < _active.Count; i++)
            {
                var definition = _active[i].Definition;
                if (definition != null && definition.Id == effectId)
                    return _active[i];
            }

            return null;
        }

        private void RemoveInstance(int instanceId, StatusEffectRemoveReason reason)
        {
            for (var i = 0; i < _active.Count; i++)
            {
                if (_active[i].InstanceId != instanceId)
                    continue;

                var effectId = _active[i].Definition?.Id;
                _active.RemoveAt(i);
                _bus.Publish(new StatusEffectRemovedEvent(instanceId, reason));
                Debug.Log($"[StatusEffectService] Removed {effectId} ({reason}).");
                return;
            }
        }
    }
}
