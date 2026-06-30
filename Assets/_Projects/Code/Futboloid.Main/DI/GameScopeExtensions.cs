using Futboloid.Core;
using Futboloid.Gameplay.Defenders;
using Futboloid.Gameplay.Input;
using Futboloid.Gameplay.Match;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Main.DI
{
    public static class GameScopeExtensions
    {
        public static IContainerBuilder RegisterGameScope(this IContainerBuilder builder)
        {
            builder.Register<MatchFlow>(Lifetime.Singleton);
            builder.Register<PitchStateMachine>(Lifetime.Singleton);
            builder.Register<DefenderPromotionService>(Lifetime.Singleton);
            builder.Register<DefenderReshuffleService>(Lifetime.Singleton);

            var inputHost = Object.FindAnyObjectByType<GameplayInputHost>();
            if (inputHost != null)
                builder.RegisterComponent(inputHost).As<IGameplayInput>();
            else
                Debug.LogError("[GameScope] GameplayInputHost not found on Game scene.");

            var defenderRegistry = Object.FindAnyObjectByType<DefenderGridRegistry>();
            if (defenderRegistry != null)
                builder.RegisterComponent(defenderRegistry);
            else
                Debug.LogWarning("[GameScope] DefenderGridRegistry not found on Game scene.");

            builder.RegisterBuildCallback(OnGameScopeBuilt);

            return builder;
        }

        private static void OnGameScopeBuilt(IObjectResolver resolver)
        {
            resolver.Resolve<PitchStateMachine>();
            resolver.Resolve<DefenderPromotionService>();
            resolver.Resolve<DefenderReshuffleService>();

            var scene = SceneManager.GetSceneByName(GameScenes.Game);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError($"[GameScope] Scene '{GameScenes.Game}' is not loaded.");
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
                resolver.InjectGameObject(root);
        }
    }
}
