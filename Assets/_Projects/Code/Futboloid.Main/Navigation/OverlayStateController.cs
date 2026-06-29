using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Gameplay.Bus.Events;
using Futboloid.Main.Session;
using UnityEngine;

namespace Futboloid.Main.Navigation
{
    public class OverlayStateController
    {
        private readonly GameSession _session;

        public NavigationState Current { get; private set; }

        public OverlayStateController(GameSession session)
        {
            _session = session;
        }

        public UniTask SetState(NavigationState next)
        {
            if (Current == next)
                return UniTask.CompletedTask;

            var previous = Current;
            Current = next;

            ApplyState(previous, next);
            _session.Bus?.Publish(new NavigationChangedEvent(previous, next));

            Debug.Log($"[OverlayStateController] {previous} → {next}");
            return UniTask.CompletedTask;
        }

        private void ApplyState(NavigationState previous, NavigationState next)
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
