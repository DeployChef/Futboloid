using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.UI;
using UnityEngine;

namespace Futboloid.Main.Navigation
{
    public class OverlayStateController
    {
        private readonly IGameEventBus _bus;
        private readonly UIService _uiService;
        private readonly ITournamentRunService _tournamentRun;

        private bool _initialized;

        public NavigationState Current { get; private set; }
        public bool IsMatchPausedInMenu { get; private set; }

        public OverlayStateController(
            IGameEventBus bus,
            UIService uiService,
            ITournamentRunService tournamentRun)
        {
            _bus = bus;
            _uiService = uiService;
            _tournamentRun = tournamentRun;
        }

        public UniTask SetState(NavigationState next)
        {
            if (_initialized && Current == next)
                return UniTask.CompletedTask;

            var isColdStart = !_initialized;
            var previous = _initialized ? Current : next;
            _initialized = true;
            Current = next;

            if (next == NavigationState.MainMenu)
                IsMatchPausedInMenu = previous == NavigationState.OnField;

            ApplyState(next, previous, isColdStart);
            _uiService.ApplyNavigation(next, IsMatchPausedInMenu);
            _bus.Publish(new NavigationChangedEvent(previous, next, IsMatchPausedInMenu));

            Debug.Log($"[OverlayStateController] {previous} → {next}");
            return UniTask.CompletedTask;
        }

        private void ApplyState(NavigationState next, NavigationState previous, bool isColdStart)
        {
            switch (next)
            {
                case NavigationState.MainMenu:
                    Time.timeScale = 0f;
                    break;

                case NavigationState.OnField:
                    Time.timeScale = 1f;
                    var resumingFromPause = previous == NavigationState.MainMenu && IsMatchPausedInMenu;
                    var newRunFromMenu = previous == NavigationState.MainMenu && !IsMatchPausedInMenu;
                    if (!resumingFromPause)
                    {
                        if (newRunFromMenu || isColdStart)
                            _tournamentRun.ResetRun();
                        _bus.Publish(new PitchResetRequestedEvent());
                    }
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
