using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.UI.Views.Tournament;
using UnityEngine;
using VContainer;

namespace Futboloid.Main.UI
{
    /// <summary>
    /// Турнирный оверлей на сцене Game — часть поля, не Root-меню.
    /// </summary>
    public class TournamentController : MonoBehaviour
    {
        [SerializeField] private TournamentWidget widget;

        private readonly List<IDisposable> _subscriptions = new();

        [Inject]
        public void Construct(IGameEventBus bus, IGameDirector director)
        {
            if (widget == null)
                widget = GetComponentInChildren<TournamentWidget>(true);

            widget?.BindDirector(director);

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
