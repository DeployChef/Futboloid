using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Input;
using Futboloid.Gameplay.Match;
using Futboloid.Gameplay.Scene;
using Futboloid.Main.DI;
using Futboloid.Main.Navigation;
using Futboloid.Main.Session;
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
        private readonly GameSession _gameSession;
        private MatchEndHandler _matchEndHandler;

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
            _matchEndHandler = _parentLifetimeScope.Container.Resolve<MatchEndHandler>();
            _matchEndHandler.Bind(bus);

            var director = _parentLifetimeScope.Parent.Container.Resolve<IGameDirector>();
            InitializeSceneViews(bus, director);

            Debug.Log("[GameState] Game scope ready, views initialized.");
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            _matchEndHandler?.Unbind();
            _matchEndHandler = null;
            _gameSession.ClearGameScope();

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
