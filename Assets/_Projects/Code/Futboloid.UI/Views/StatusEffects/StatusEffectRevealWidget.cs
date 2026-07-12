using System;
using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Pause;
using Futboloid.Core.StatusEffects;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Futboloid.UI.Views.StatusEffects
{
    /// <summary>
    /// Показывает карточку timed-эффекта при первом получении за сессию приложения.
    /// Space — продолжить. Ставит игру на паузу, пока карточка открыта.
    /// </summary>
    public class StatusEffectRevealWidget : MonoBehaviour
    {
        [SerializeField] private StatusEffectRevealCardView card;
        [SerializeField] private GameObject continueHintRoot;

        private readonly List<IDisposable> _subscriptions = new();
        private readonly Queue<StatusEffectAppliedEvent> _pendingReveals = new();

        private IStatusEffectRevealMemory _revealMemory;
        private PauseCoordinator _pause;
        private bool _pausedForReveal;
        private bool _isShowing;

        private void Awake() => SetVisible(false);

        [Inject]
        public void Construct(
            IGameEventBus bus,
            PauseCoordinator pause,
            IStatusEffectRevealMemory revealMemory)
        {
            _pause = pause;
            _revealMemory = revealMemory;

            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            _subscriptions.Add(bus.Subscribe<StatusEffectAppliedEvent>(OnStatusEffectApplied));
            _subscriptions.Add(bus.Subscribe<PitchResetRequestedEvent>(_ => CancelPendingReveals()));
            _subscriptions.Add(bus.Subscribe<MatchEndedEvent>(_ => CancelPendingReveals()));
        }

        private void Update()
        {
            if (!_isShowing)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.spaceKey.wasPressedThisFrame)
                return;

            DismissCurrent();
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            ReleasePause();
        }

        private void OnStatusEffectApplied(StatusEffectAppliedEvent e)
        {
            if (string.IsNullOrEmpty(e.EffectId) || _revealMemory.WasRevealed(e.EffectId))
                return;

            _revealMemory.MarkRevealed(e.EffectId);
            _pendingReveals.Enqueue(e);
            TryShowNext();
        }

        private void TryShowNext()
        {
            if (_isShowing || _pendingReveals.Count == 0)
                return;

            var reveal = _pendingReveals.Dequeue();
            _isShowing = true;
            RequestPause();
            SetVisible(true);
            card?.Show(reveal);

            if (continueHintRoot != null)
                continueHintRoot.SetActive(true);
        }

        private void DismissCurrent()
        {
            _isShowing = false;
            card?.Hide();

            if (_pendingReveals.Count > 0)
            {
                TryShowNext();
                return;
            }

            SetVisible(false);
            ReleasePause();
        }

        private void CancelPendingReveals()
        {
            _pendingReveals.Clear();
            _isShowing = false;
            card?.Hide();
            SetVisible(false);
            ReleasePause();
        }

        private void RequestPause()
        {
            if (_pausedForReveal || _pause == null)
                return;

            _pausedForReveal = true;
            _pause.Request(PauseReasons.StatusEffectReveal);
        }

        private void ReleasePause()
        {
            if (!_pausedForReveal || _pause == null)
                return;

            _pausedForReveal = false;
            _pause.Release(PauseReasons.StatusEffectReveal);
        }

        private void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);

            if (!visible && continueHintRoot != null)
                continueHintRoot.SetActive(false);
        }
    }
}
