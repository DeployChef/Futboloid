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

        public NavigationState Current { get; private set; }

        public OverlayStateController(GameSession session, UIService uiService)
        {
            _session = session;
            _uiService = uiService;
        }

        public UniTask SetState(NavigationState next)
        {
            if (Current == next)
                return UniTask.CompletedTask;

            var previous = Current;
            Current = next;

            ApplyState(next);
            _uiService.ApplyNavigation(next);
            _session.Bus?.Publish(new NavigationChangedEvent(previous, next));

            Debug.Log($"[OverlayStateController] {previous} → {next}");
            return UniTask.CompletedTask;
        }

        private void ApplyState(NavigationState next)
        {
            switch (next)
            {
                case NavigationState.MainMenu:
                    Time.timeScale = 1f;
                    break;

                case NavigationState.OnField:
                    Time.timeScale = 1f;
                    _session.Pitch?.EnterKickoffWait();
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
