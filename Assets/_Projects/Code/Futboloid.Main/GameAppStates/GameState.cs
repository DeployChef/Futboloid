using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Gameplay.Input;
using Futboloid.Gameplay.Scene;
using Futboloid.Main.DI;
using Futboloid.Main.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Main.GameAppStates
{
    public class GameState
    {
        private readonly LifetimeScope _parentLifetimeScope;

        public LifetimeScope LifetimeScope { get; private set; }

        public GameState(LifetimeScope parentLifetimeScope)
        {
            _parentLifetimeScope = parentLifetimeScope;
        }

        public UniTask Enter()
        {
            LifetimeScope = _parentLifetimeScope.CreateChild(builder => builder.RegisterGameScope());

            var bus = _parentLifetimeScope.Container.Resolve<IGameEventBus>();
            var director = _parentLifetimeScope.Parent.Container.Resolve<IGameDirector>();
            InitializeSceneViews(bus, director);

            Debug.Log("[GameState] Game scope ready, views initialized.");
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            if (LifetimeScope != null)
            {
                LifetimeScope.Dispose();
                LifetimeScope = null;
            }

            Debug.Log("[GameState] Game scope disposed.");
            return UniTask.CompletedTask;
        }

        private void InitializeSceneViews(IGameEventBus bus, IGameDirector director)
        {
            var gameScene = SceneManager.GetActiveScene();
            var roots = gameScene.GetRootGameObjects();
            var count = 0;
            var input = FindGameplayInput();

            if (input == null)
                Debug.LogError("[GameState] GameplayInputHost not found on Game scene.");

            foreach (var root in roots)
            {
                foreach (var initializable in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (initializable is TournamentController tournamentController)
                        tournamentController.BindDirector(director);

                    if (initializable is IGameSceneInitializable sceneInit)
                    {
                        sceneInit.Initialize(bus);
                        count++;
                    }

                    if (input != null && initializable is IGameplayInputConsumer inputConsumer)
                        inputConsumer.BindInput(input);
                }
            }

            Debug.Log($"[GameState] IGameSceneInitializable count: {count}");
        }

        private static IGameplayInput FindGameplayInput()
        {
            return Object.FindAnyObjectByType<GameplayInputHost>();
        }
    }
}
