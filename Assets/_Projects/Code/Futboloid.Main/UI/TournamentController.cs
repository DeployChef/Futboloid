using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Scene;
using Futboloid.UI.Views.Tournament;
using UnityEngine;

namespace Futboloid.Main.UI
{
    /// <summary>
    /// Турнирный оверлей на сцене Game — часть поля, не Root-меню.
    /// </summary>
    public class TournamentController : MonoBehaviour, IGameSceneInitializable
    {
        [SerializeField] private TournamentWidget widget;

        private IGameDirector _director;
        private readonly List<IDisposable> _subscriptions = new();

        public void BindDirector(IGameDirector director)
        {
            _director = director;

            if (widget == null)
                widget = GetComponentInChildren<TournamentWidget>(true);

            widget?.BindDirector(director);
        }

        public void Initialize(IGameEventBus bus)
        {
            if (widget == null)
                widget = GetComponentInChildren<TournamentWidget>(true);

            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            SetVisible(false);
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnNavigationChanged(NavigationChangedEvent e) =>
            SetVisible(e.Current == NavigationState.Tournament);

        private void SetVisible(bool visible)
        {
            if (widget == null)
                return;

            if (visible)
                widget.Open();
            else
                widget.Close();
        }
    }
}
