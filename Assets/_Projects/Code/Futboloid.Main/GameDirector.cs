using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Main.DI;
using Futboloid.Main.GameAppStates;
using Futboloid.Main.Navigation;
using UnityEngine;

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

        public void InitializeGame()
        {
            RunInitializeAsync().Forget();
        }

        public void RestartTournament() =>
            Debug.LogWarning("[GameDirector] RestartTournament — not implemented yet.");

        public void ReturnToMainMenu() =>
            _overlay.SetState(NavigationState.MainMenu).Forget();

        public void SaveGame() =>
            Debug.LogWarning("[GameDirector] SaveGame — not implemented yet.");

        public void LoadLastSave() =>
            Debug.LogWarning("[GameDirector] LoadLastSave — not implemented yet.");

        private async UniTaskVoid RunInitializeAsync()
        {
            Debug.Log("[GameDirector] Cold start…");

            var appRoot = new AppRootState(_rootLifetimeScope);
            await appRoot.Enter();

            _appGameState = appRoot.AppGameState;
            await _appGameState.Enter();
            _overlay = _appGameState.Overlay;

            Debug.Log("[GameDirector] Infrastructure ready (Root → App → Game → MainMenu).");
        }
    }
}
