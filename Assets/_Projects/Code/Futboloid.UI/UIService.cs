using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.UI.Views.MainMenu;
using Futboloid.UI.Views.PauseMenu;
using Futboloid.UI.Views.Settings;
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

        public bool IsOpen<T>() where T : class, IWidget
        {
            if (!_widgets.TryGetValue(typeof(T), out var widget))
                return false;

            return widget is Component component && component.gameObject.activeSelf;
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

        public void ApplyNavigation(NavigationState state, bool isMatchPausedInMenu = false)
        {
            switch (state)
            {
                case NavigationState.MainMenu:
                    Close<SettingsView>();
                    Close<PauseMenuView>();
                    ShowMainMenu(isMatchPausedInMenu);
                    break;

                case NavigationState.OnField:
                    Close<SettingsView>();
                    Close<MainMenuWidget>();
                    Close<PauseMenuView>();
                    break;

                case NavigationState.Tournament:
                    Close<SettingsView>();
                    Close<MainMenuWidget>();
                    Close<PauseMenuView>();
                    break;

                case NavigationState.Pause:
                    Show<PauseMenuView>();
                    break;
            }
        }

        private void ShowMainMenu(bool showContinue)
        {
            if (_widgets.TryGetValue(typeof(MainMenuWidget), out var widget) && widget is MainMenuWidget mainMenu)
            {
                mainMenu.Open(showContinue);
                return;
            }

            Debug.Log("[UIService] Show<MainMenuWidget> — prefab позже");
        }
    }
}
