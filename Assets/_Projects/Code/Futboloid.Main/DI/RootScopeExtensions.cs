using Futboloid.Main.Navigation;
using Futboloid.UI;
using Futboloid.UI.Views.MainMenu;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Main.DI
{
    public static class RootScopeExtensions
    {
        public static IContainerBuilder RegisterRootScope(this IContainerBuilder builder)
        {
            builder.Register<MatchHudWidget>(Lifetime.Singleton);
            builder.Register<PauseWidget>(Lifetime.Singleton);
            builder.Register<TournamentWidget>(Lifetime.Singleton);
            builder.Register<UIService>(Lifetime.Singleton);
            builder.RegisterBuildCallback(RegisterUiWidgets);

            return builder;
        }

        public static IContainerBuilder RegisterRootSceneUi(this IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<MainMenuWidget>();
            builder.RegisterComponentInHierarchy<MainMenuLayout>();
            builder.RegisterComponentInHierarchy<NavigationInputHost>();

            return builder;
        }

        private static void RegisterUiWidgets(IObjectResolver container)
        {
            var ui = container.Resolve<UIService>();
            ui.Register(container.Resolve<MainMenuWidget>());
            ui.Register(container.Resolve<MatchHudWidget>());
            ui.Register(container.Resolve<PauseWidget>());
            ui.Register(container.Resolve<TournamentWidget>());
        }
    }
}
