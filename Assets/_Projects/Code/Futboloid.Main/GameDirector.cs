using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Run;
using Futboloid.Main.DI;
using Futboloid.Main.GameAppStates;
using Futboloid.Main.Navigation;
using UnityEngine;
using VContainer;

namespace Futboloid.Main
{
    public class GameDirector : IGameDirector
    {
        private readonly RootLifetimeScope _rootLifetimeScope;
        private AppGameState _appGameState;
        private OverlayStateController _overlay;

        public GameDirector(RootLifetimeScope rootLifetimeScope)
        {
            _rootLifetimeScope = rootLifetimeScope;
        }

        public RootLifetimeScope Root => _rootLifetimeScope;

        public NavigationState CurrentNavigation =>
            _overlay?.Current ?? NavigationState.MainMenu;

        public bool IsMatchPausedInMenu =>
            _overlay != null && _overlay.IsMatchPausedInMenu;

        public ITournamentBracketReadModel TournamentBracket { get; private set; }

        public async UniTask InitializeGameAsync()
        {
            Debug.Log("[GameDirector] Cold start…");

            var appRoot = new AppRootState(_rootLifetimeScope);
            await appRoot.Enter();

            _appGameState = appRoot.AppGameState;
            await _appGameState.Enter();
            _overlay = _appGameState.Overlay;
            TournamentBracket = _appGameState.LifetimeScope.Container.Resolve<ITournamentBracketReadModel>();

            Debug.Log("[GameDirector] Infrastructure ready (Root → App → Game → OnField).");
        }

        public void GoOnField() =>
            _overlay.SetState(NavigationState.OnField).Forget();

        public void ReturnToPause() =>
            _overlay.SetState(NavigationState.Pause).Forget();

        public void RestartTournament()
        {
            if (_appGameState?.LifetimeScope == null)
                return;

            var container = _appGameState.LifetimeScope.Container;
            
            // Сначала перки/XP, потом забег — чтобы TournamentRunStarted уже видел сброс.
            var run = container.Resolve<ITournamentRunService>();
            var progression = container.Resolve<IRunProgressionService>();
            progression.Reset();
            run.ResetRun();

            // Явно сбрасываем поле и таймер
            var bus = container.Resolve<IGameEventBus>();
            bus.Publish(new PitchResetRequestedEvent());

            // Переходим на поле
            _overlay.SetState(NavigationState.OnField).Forget();
        }

        public void ReturnToMainMenu() =>
            _overlay.SetState(NavigationState.MainMenu).Forget();

        public void SaveGame() =>
            Debug.LogWarning("[GameDirector] SaveGame — not implemented yet.");

        public void LoadLastSave() =>
            Debug.LogWarning("[GameDirector] LoadLastSave — not implemented yet.");
    }
}
