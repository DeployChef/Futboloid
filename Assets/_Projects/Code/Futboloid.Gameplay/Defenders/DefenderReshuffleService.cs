using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Match;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>После гола: живые футболисты бегут на home, затем Pitch → KickoffWait.</summary>
    public sealed class DefenderReshuffleService : IDisposable
    {
        private readonly IGameEventBus _bus;
        private readonly PitchStateMachine _pitch;
        private readonly DefenderGridRegistry _registry;
        private readonly List<IDisposable> _subscriptions = new();
        private readonly HashSet<int> _pendingSlotIds = new();

        public DefenderReshuffleService(
            IGameEventBus bus,
            PitchStateMachine pitch,
            DefenderGridRegistry registry)
        {
            _bus = bus;
            _pitch = pitch;
            _registry = registry;
            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<DefenderReturnedHomeEvent>(OnDefenderReturnedHome));
            _subscriptions.Add(bus.Subscribe<DefenderDestroyedEvent>(OnDefenderDestroyed));
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            if (e.Phase != PitchPhase.Reshuffle)
                return;

            BeginReshuffle();
        }

        private void BeginReshuffle()
        {
            _pendingSlotIds.Clear();

            if (_registry == null)
            {
                TryCompleteReshuffle();
                return;
            }

            _registry.ForEachLiving(defender =>
            {
                if (!defender.BeginReshuffleReturn(
                        _registry.ReshuffleSpeed,
                        _registry.ReshuffleAcceleration,
                        _registry.ArriveThreshold))
                    return;

                _pendingSlotIds.Add(defender.SlotId);
            });

            TryCompleteReshuffle();
        }

        private void OnDefenderReturnedHome(DefenderReturnedHomeEvent e)
        {
            if (!_pendingSlotIds.Remove(e.SlotId))
                return;

            TryCompleteReshuffle();
        }

        private void OnDefenderDestroyed(DefenderDestroyedEvent e)
        {
            if (!_pendingSlotIds.Remove(e.SlotId))
                return;

            TryCompleteReshuffle();
        }

        private void TryCompleteReshuffle()
        {
            if (_pendingSlotIds.Count > 0)
                return;

            if (_pitch.Current == PitchPhase.Reshuffle)
                _pitch.CompleteReshuffle();
        }
    }
}
