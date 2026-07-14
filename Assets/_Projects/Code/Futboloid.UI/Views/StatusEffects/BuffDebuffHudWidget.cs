using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;
using VContainer;

namespace Futboloid.UI.Views.StatusEffects
{
    /// <summary>
    /// Вертикальный стек timed-эффектов: иконка, кольцо таймера, тултип по наведению.
    /// </summary>
    public class BuffDebuffHudWidget : MonoBehaviour
    {
        [SerializeField] private Transform slotRoot;
        [SerializeField] private StatusEffectIconSlot slotPrefab;
        [SerializeField] private StatusEffectTooltipWidget tooltip;

        private readonly List<IDisposable> _subscriptions = new();
        private readonly Dictionary<int, StatusEffectIconSlot> _slots = new();
        private readonly List<StatusEffectIconSlot> _slotPool = new();

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
            _subscriptions.Add(bus.Subscribe<StatusEffectAppliedEvent>(OnApplied));
            _subscriptions.Add(bus.Subscribe<StatusEffectRemovedEvent>(OnRemoved));
            _subscriptions.Add(bus.Subscribe<StatusEffectRefreshedEvent>(OnRefreshed));

            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnNavigationChanged(NavigationChangedEvent e) =>
            gameObject.SetActive(e.Current != NavigationState.Tournament);

        private void OnApplied(StatusEffectAppliedEvent e)
        {
            if (slotPrefab == null || slotRoot == null)
            {
                Debug.LogWarning(
                    "[BuffDebuffHudWidget] Assign Slot Prefab and Slot Root on BuffDebuffHudWidget.");
                return;
            }

            if (_slots.ContainsKey(e.InstanceId))
                return;

            var slot = RentSlot();
            slot.Bind(e, tooltip);
            _slots[e.InstanceId] = slot;
        }

        private void OnRemoved(StatusEffectRemovedEvent e)
        {
            if (!_slots.TryGetValue(e.InstanceId, out var slot))
                return;

            slot.Hide();
            _slots.Remove(e.InstanceId);
        }

        private void OnRefreshed(StatusEffectRefreshedEvent e)
        {
            if (_slots.TryGetValue(e.InstanceId, out var slot))
                slot.RefreshRing(e.DurationSeconds);
        }

        private StatusEffectIconSlot RentSlot()
        {
            for (var i = 0; i < _slotPool.Count; i++)
            {
                if (!_slotPool[i].gameObject.activeSelf)
                    return _slotPool[i];
            }

            var slot = Instantiate(slotPrefab, slotRoot);
            _slotPool.Add(slot);
            return slot;
        }
    }
}
