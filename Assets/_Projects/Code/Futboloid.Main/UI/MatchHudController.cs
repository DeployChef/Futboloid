using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.UI.Views.MatchHud;
using UnityEngine;
using VContainer;

namespace Futboloid.Main.UI
{
    /// <summary>
    /// Match HUD на сцене Game — часть поля. Меню и пауза (Root overlay) не скрывают HUD;
    /// прячем только когда уходим с поля (Tournament и т.п.).
    /// </summary>
    public class MatchHudController : MonoBehaviour
    {
        [SerializeField] private MatchHudWidget hud;

        private readonly List<IDisposable> _subscriptions = new();

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            if (hud == null)
                hud = GetComponentInChildren<MatchHudWidget>(true);

            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
            _subscriptions.Add(bus.Subscribe<MatchTimerChangedEvent>(OnTimerChanged));
            _subscriptions.Add(bus.Subscribe<MatchScoreChangedEvent>(OnScoreChanged));

            hud?.Open();
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnNavigationChanged(NavigationChangedEvent e) =>
            SetVisible(IsFieldHudVisible(e.Current));

        private static bool IsFieldHudVisible(NavigationState state) =>
            state != NavigationState.Tournament;

        private void OnTimerChanged(MatchTimerChangedEvent e) =>
            hud?.SetTimer(e.Normalized, e.RemainingSeconds);

        private void OnScoreChanged(MatchScoreChangedEvent e) =>
            hud?.SetScore(e.PlayerScore, e.OpponentScore);

        private void SetVisible(bool visible)
        {
            if (hud == null)
                return;

            if (visible)
                hud.Open();
            else
                hud.Close();
        }
    }
}
