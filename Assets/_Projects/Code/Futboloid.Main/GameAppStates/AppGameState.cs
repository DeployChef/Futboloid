using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Audio;
using Futboloid.Core.Pause;
using Futboloid.Core.Run;
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
            LifetimeScope.Container.Resolve<IRunProgressionService>();

            var gameScene = await EnsureGameSceneAsync();
            if (!gameScene.IsValid() || !gameScene.isLoaded)
            {
                Debug.LogError($"[AppGameState] Scene '{GameScenes.Game}' not found. Is it in Build Settings?");
                return;
            }

            await WarmupAudioClipsAsync(LifetimeScope.Container.Resolve<IAudioManager>());

            SceneManager.SetActiveScene(gameScene);

            _gameState = new GameState(LifetimeScope);
            await _gameState.Enter(gameScene);

            LifetimeScope.Container.Resolve<MatchEndHandler>();

            Overlay = LifetimeScope.Container.Resolve<OverlayStateController>();
            await Overlay.SetState(NavigationState.OnField);

            Debug.Log($"[AppGameState] '{GameScenes.Game}' ready, navigation → OnField.");
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
                _parentLifetimeScope.Container.Resolve<PauseCoordinator>().ReleaseAll();
                _parentLifetimeScope.Container.Resolve<IAudioManager>().StopAll();
                LifetimeScope.Dispose();
                LifetimeScope = null;
            }

            if (TryFindLoadedGameScene(out var gameScene))
                await SceneManager.UnloadSceneAsync(gameScene).ToUniTask();

            Debug.Log($"[AppGameState] '{GameScenes.Game}' exited.");
        }

        private async UniTask<Scene> EnsureGameSceneAsync()
        {
            // Editor: additive Game в иерархии может появиться в sceneCount на следующий кадр.
            await UniTask.Yield();

            if (!TryFindLoadedGameScene(out _))
                await SceneManager.LoadSceneAsync(GameScenes.Game, LoadSceneMode.Additive).ToUniTask();

            return await KeepSingleGameSceneAsync();
        }

        private static async UniTask<Scene> KeepSingleGameSceneAsync()
        {
            Scene first = default;
            var found = false;

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name != GameScenes.Game || !scene.isLoaded)
                    continue;

                if (!found)
                {
                    first = scene;
                    found = true;
                }
                else
                {
                    await SceneManager.UnloadSceneAsync(scene).ToUniTask();
                }
            }

            return first;
        }

        private static bool TryFindLoadedGameScene(out Scene gameScene)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == GameScenes.Game && scene.isLoaded)
                {
                    gameScene = scene;
                    return true;
                }
            }

            gameScene = default;
            return false;
        }

        private static async UniTask WarmupAudioClipsAsync(IAudioManager audio)
        {
            if (audio == null)
                return;

            foreach (var clip in audio.EnumerateClips())
            {
                audio.WarmupClip(clip);
                await UniTask.Yield();
            }
        }
    }
}
