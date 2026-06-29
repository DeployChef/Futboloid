using System;
using System.Collections.Generic;
using Futboloid.Core;
using UnityEngine;

namespace Futboloid.UI
{
    public class UIService
    {
        private readonly Dictionary<Type, IWidget> _widgets = new();

        public void Register<T>(T widget) where T : class, IWidget
        {
            _widgets[typeof(T)] = widget;
        }

        public void Show<T>() where T : class, IWidget
        {
            if (_widgets.TryGetValue(typeof(T), out var widget))
            {
                widget.Open();
                return;
            }

            Debug.Log($"[UIService] Show<{typeof(T).Name}> — prefab позже");
        }

        public void Close<T>() where T : class, IWidget
        {
            if (_widgets.TryGetValue(typeof(T), out var widget))
            {
                widget.Close();
                return;
            }

            Debug.Log($"[UIService] Close<{typeof(T).Name}> — prefab позже");
        }

        public void ApplyNavigation(NavigationState state)
        {
            switch (state)
            {
                case NavigationState.MainMenu:
                    Close<MatchHudWidget>();
                    Show<MainMenuWidget>();
                    break;

                case NavigationState.OnField:
                    Close<MainMenuWidget>();
                    Close<PauseWidget>();
                    Show<MatchHudWidget>();
                    break;

                case NavigationState.Tournament:
                    Close<MatchHudWidget>();
                    Show<TournamentWidget>();
                    break;

                case NavigationState.Pause:
                    Show<PauseWidget>();
                    break;
            }
        }
    }
}
