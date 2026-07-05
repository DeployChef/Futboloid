using Futboloid.Core;
using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Defenders;
using Futboloid.Gameplay.Input;
using Futboloid.Gameplay.Keeper;
using Futboloid.Gameplay.Match;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Main.DI
{
    public static class GameScopeExtensions
    {
        public static IContainerBuilder RegisterGameScope(this IContainerBuilder builder, Scene gameScene)
        {
            builder.Register<MatchFlow>(Lifetime.Singleton);
            builder.Register<ComboScoreService>(Lifetime.Singleton);
            builder.Register<PitchStateMachine>(Lifetime.Singleton);
            builder.Register<BonusPickCoordinator>(Lifetime.Singleton);
            builder.Register<DefenderPromotionService>(Lifetime.Singleton);
            builder.Register<DefenderReshuffleService>(Lifetime.Singleton);
            builder.Register<DefenderLogic>(Lifetime.Transient);

            builder.RegisterComponentInScene<GameplayInputHost>(gameScene).As<IGameplayInput>();
            builder.RegisterComponentInScene<DefenderGridRegistry>(gameScene);
            builder.RegisterComponentInScene<DefenderSpawner>(gameScene);
            builder.RegisterComponentInScene<DefenderSlotLayout>(gameScene);
            builder.RegisterComponentInScene<PitchBounds>(gameScene);
            builder.RegisterComponentInScene<GoalAnchor>(gameScene);
            builder.RegisterComponentInScene<BallView>(gameScene);
            builder.RegisterComponentInScene<GoalkeeperView>(gameScene);

            builder.RegisterBuildCallback(resolver => OnGameScopeBuilt(resolver, gameScene));

            return builder;
        }

        private static RegistrationBuilder RegisterComponentInScene<T>(
            this IContainerBuilder builder,
            Scene scene) where T : Component
        {
            var component = FindInScene<T>(scene);
            if (component == null)
                Debug.LogError($"[GameScope] {typeof(T).Name} not found in scene '{scene.name}'.");

            return builder.RegisterComponent(component);
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            if (!scene.IsValid())
                return null;

            foreach (var root in scene.GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                    return component;
            }

            return null;
        }

        private static void OnGameScopeBuilt(IObjectResolver resolver, Scene gameScene)
        {
            resolver.Resolve<PitchStateMachine>();
            resolver.Resolve<ComboScoreService>();
            resolver.Resolve<BonusPickCoordinator>();
            resolver.Resolve<DefenderPromotionService>();
            resolver.Resolve<DefenderReshuffleService>();

            if (!gameScene.IsValid() || !gameScene.isLoaded)
                return;

            foreach (var root in gameScene.GetRootGameObjects())
                resolver.InjectGameObject(root);
        }
    }
}
