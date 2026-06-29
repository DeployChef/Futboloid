using Cysharp.Threading.Tasks;
using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Match;
using Futboloid.Gameplay.Scene;
using Futboloid.Main.DI;
using Futboloid.Main.Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Main.GameAppStates
{
    public class GameState
    {
        private readonly LifetimeScope _parentLifetimeScope;
        private readonly GameSession _gameSession;

        public LifetimeScope LifetimeScope { get; private set; }

        public GameState(LifetimeScope parentLifetimeScope, GameSession gameSession)
        {
            _parentLifetimeScope = parentLifetimeScope;
            _gameSession = gameSession;
        }

        public UniTask Enter()
        {
            LifetimeScope = _parentLifetimeScope.CreateChild(builder => builder.RegisterGameScope());

            var bus = LifetimeScope.Container.Resolve<IGameEventBus>();
            var matchFlow = LifetimeScope.Container.Resolve<MatchFlow>();
            var pitch = LifetimeScope.Container.Resolve<PitchStateMachine>();

            _gameSession.BindGameScope(bus, matchFlow, pitch);
            InitializeSceneViews(bus);

            Debug.Log("[GameState] Game scope ready, views initialized.");
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            _gameSession.ClearGameScope();

            if (LifetimeScope != null)
            {
                LifetimeScope.Dispose();
                LifetimeScope = null;
            }

            Debug.Log("[GameState] Game scope disposed.");
            return UniTask.CompletedTask;
        }

        private static void InitializeSceneViews(IGameEventBus bus)
        {
            var gameScene = SceneManager.GetActiveScene();
            var roots = gameScene.GetRootGameObjects();
            var count = 0;

            foreach (var root in roots)
            {
                foreach (var initializable in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (initializable is not IGameSceneInitializable sceneInit)
                        continue;

                    sceneInit.Initialize(bus);
                    count++;
                }
            }

            Debug.Log($"[GameState] IGameSceneInitializable count: {count}");
        }
    }
}
