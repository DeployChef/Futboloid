using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Pause;
using Futboloid.Core.Run;
using Futboloid.UI;
using UnityEngine;

namespace Futboloid.Main.Navigation
{
    public class OverlayStateController
    {
        private readonly IGameEventBus _bus;
        private readonly UIService _uiService;
        private readonly ITournamentRunService _tournamentRun;
        private readonly IRunProgressionService _runProgression;
        private readonly PauseCoordinator _pause;

        private bool _initialized;

        public NavigationState Current { get; private set; }
        public bool IsMatchPausedInMenu { get; private set; }

        public OverlayStateController(
            IGameEventBus bus,
            UIService uiService,
            ITournamentRunService tournamentRun,
            IRunProgressionService runProgression,
            PauseCoordinator pause)
        {
            _bus = bus;
            _uiService = uiService;
            _tournamentRun = tournamentRun;
            _runProgression = runProgression;
            _pause = pause;
        }

        public UniTask SetState(NavigationState next)
        {
            if (_initialized && Current == next)
                return UniTask.CompletedTask;

            var isColdStart = !_initialized;
            var previous = _initialized ? Current : next;
            _initialized = true;

            // Сохраняем флаг ДО ApplyState, который его сбрасывает
            var wasPausedInMenu = IsMatchPausedInMenu;

            Current = next;

            if (next == NavigationState.MainMenu)
                IsMatchPausedInMenu = previous == NavigationState.OnField;

            ApplyState(next, previous, isColdStart);
            _uiService.ApplyNavigation(next, IsMatchPausedInMenu);

            var resumingPausedMatch = next == NavigationState.OnField
                && previous == NavigationState.MainMenu
                && wasPausedInMenu;
            _bus.Publish(new NavigationChangedEvent(previous, next, IsMatchPausedInMenu, resumingPausedMatch));

            Debug.Log($"[OverlayStateController] {previous} → {next}");
            return UniTask.CompletedTask;
        }

        private void ApplyState(NavigationState next, NavigationState previous, bool isColdStart)
        {
            SyncNavigationPause(next);

            switch (next)
            {
                case NavigationState.OnField:
                    var resumingFromPause = previous == NavigationState.Pause;
                    var resumingFromMenu = previous == NavigationState.MainMenu && IsMatchPausedInMenu;
                    var newRunFromMenu = previous == NavigationState.MainMenu && !IsMatchPausedInMenu;
                    if (!resumingFromPause && !resumingFromMenu)
                    {
                        if (newRunFromMenu || isColdStart)
                        {
                            _tournamentRun.ResetRun();
                            _runProgression.Reset();
                        }
                        _bus.Publish(new PitchResetRequestedEvent());
                    }
                    else if (resumingFromMenu)
                        IsMatchPausedInMenu = false;
                    break;
            }
        }

        private void SyncNavigationPause(NavigationState state)
        {
            _pause.Release(PauseReasons.MainMenu);
            _pause.Release(PauseReasons.EscapePause);

            switch (state)
            {
                case NavigationState.MainMenu:
                    _pause.Request(PauseReasons.MainMenu);
                    break;
                case NavigationState.Pause:
                    _pause.Request(PauseReasons.EscapePause);
                    break;
            }
        }
    }
}
