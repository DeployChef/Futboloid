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
        private bool _gameSceneIsLoaded;

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

            var gameScene = await EnsureGameSceneAsync();
            if (!gameScene.IsValid())
            {
                Debug.LogError($"[AppGameState] Scene '{GameScenes.Game}' not found. Is it in Build Settings?");
                return;
            }

            SceneManager.SetActiveScene(gameScene);

            var gameSession = LifetimeScope.Container.Resolve<GameSession>();
            _gameState = new GameState(LifetimeScope, gameSession);
            await _gameState.Enter();

            Overlay = LifetimeScope.Container.Resolve<OverlayStateController>();
            await Overlay.SetState(NavigationState.MainMenu);

            Debug.Log($"[AppGameState] '{GameScenes.Game}' ready, navigation → MainMenu.");
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

            await UnloadGameSceneIfNeededAsync();

            Debug.Log($"[AppGameState] '{GameScenes.Game}' exited.");
        }

        private async UniTask<Scene> EnsureGameSceneAsync()
        {
            var gameScene = SceneManager.GetSceneByName(GameScenes.Game);
            _gameSceneIsLoaded = gameScene.IsValid();

            if (_gameSceneIsLoaded)
            {
                Debug.Log($"[AppGameState] '{GameScenes.Game}' already loaded — skip additive load.");
                return gameScene;
            }

            await SceneManager.LoadSceneAsync(GameScenes.Game, LoadSceneMode.Additive).ToUniTask();
            return SceneManager.GetSceneByName(GameScenes.Game);
        }

        private async UniTask UnloadGameSceneIfNeededAsync()
        {
            if (_gameSceneIsLoaded)
                return;

            var gameScene = SceneManager.GetSceneByName(GameScenes.Game);
            if (gameScene.IsValid())
                await SceneManager.UnloadSceneAsync(gameScene).ToUniTask();
        }
    }
}
