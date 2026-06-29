using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Main.DI;
using Futboloid.Main.Navigation;
using Futboloid.Main.Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Main.GameAppStates
{
    public class AppGameState
    {
        private readonly LifetimeScope _parentLifetimeScope;
        private GameState _gameState;

        public LifetimeScope LifetimeScope { get; private set; }
        public GameState GameState => _gameState;
        public OverlayStateController Overlay { get; private set; }

        public AppGameState(LifetimeScope parentLifetimeScope)
        {
            _parentLifetimeScope = parentLifetimeScope;
        }

        public async UniTask Enter()
        {
            LifetimeScope = _parentLifetimeScope.CreateChild(builder => builder.RegisterAppScope());

            await SceneManager.LoadSceneAsync(GameScenes.Game, LoadSceneMode.Additive).ToUniTask();

            var gameScene = SceneManager.GetSceneByName(GameScenes.Game);
            if (!gameScene.IsValid())
            {
                Debug.LogError($"[AppGameState] Scene '{GameScenes.Game}' not found after load. Is it in Build Settings?");
                return;
            }

            SceneManager.SetActiveScene(gameScene);

            var gameSession = LifetimeScope.Container.Resolve<GameSession>();
            _gameState = new GameState(LifetimeScope, gameSession);
            await _gameState.Enter();

            Overlay = LifetimeScope.Container.Resolve<OverlayStateController>();
            await Overlay.SetState(NavigationState.MainMenu);

            Debug.Log($"[AppGameState] '{GameScenes.Game}' loaded, navigation → MainMenu.");
        }

        public async UniTask Exit()
        {
            if (_gameState != null)
            {
                await _gameState.Exit();
                _gameState = null;
            }

            if (LifetimeScope != null)
            {
                LifetimeScope.Dispose();
                LifetimeScope = null;
            }

            var gameScene = SceneManager.GetSceneByName(GameScenes.Game);
            if (gameScene.IsValid())
                await SceneManager.UnloadSceneAsync(gameScene).ToUniTask();

            Debug.Log($"[AppGameState] '{GameScenes.Game}' unloaded.");
        }
    }
}
