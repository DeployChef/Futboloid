using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Gameplay.Bus.Events;
using Futboloid.Main.Session;
using Futboloid.UI;
using UnityEngine;

namespace Futboloid.Main.Navigation
{
    public class OverlayStateController
    {
        private readonly GameSession _session;
        private readonly UIService _uiService;

        private bool _initialized;

        public NavigationState Current { get; private set; }
        public bool IsMatchPausedInMenu { get; private set; }

        public OverlayStateController(GameSession session, UIService uiService)
        {
            _session = session;
            _uiService = uiService;
        }

        public UniTask SetState(NavigationState next)
        {
            if (_initialized && Current == next)
                return UniTask.CompletedTask;

            var previous = _initialized ? Current : next;
            _initialized = true;
            Current = next;

            if (next == NavigationState.MainMenu)
                IsMatchPausedInMenu = previous == NavigationState.OnField;

            ApplyState(next, previous);
            _uiService.ApplyNavigation(next, IsMatchPausedInMenu);
            _session.Bus?.Publish(new NavigationChangedEvent(previous, next, IsMatchPausedInMenu));

            Debug.Log($"[OverlayStateController] {previous} → {next}");
            return UniTask.CompletedTask;
        }

        private void ApplyState(NavigationState next, NavigationState previous)
        {
            switch (next)
            {
                case NavigationState.MainMenu:
                    Time.timeScale = 0f;
                    break;

                case NavigationState.OnField:
                    Time.timeScale = 1f;
                    var resumingFromPause = previous == NavigationState.MainMenu && IsMatchPausedInMenu;
                    if (!resumingFromPause)
                        _session.Pitch?.Reset();
                    else
                        IsMatchPausedInMenu = false;
                    break;

                case NavigationState.Tournament:
                    Time.timeScale = 1f;
                    break;

                case NavigationState.Pause:
                    Time.timeScale = 0f;
                    break;
            }
        }
    }
}
