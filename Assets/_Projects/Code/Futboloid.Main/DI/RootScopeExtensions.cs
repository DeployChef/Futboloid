using Futboloid.Main.Navigation;
using Futboloid.Core.Audio;
using Futboloid.Core.Localization;
using Futboloid.Main.Audio;
using Futboloid.Main.Localization;
using Futboloid.UI;
using Futboloid.UI.Views.MainMenu;
using Futboloid.UI.Views.PauseMenu;
using Futboloid.UI.Views.Settings;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Main.DI
{
    public static class RootScopeExtensions
    {
        public static IContainerBuilder RegisterRootScope(this IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<AudioManager>().As<IAudioManager>();
            builder.Register<LocalizationService>(Lifetime.Singleton)
                .AsSelf()
                .As<ILocalizationService>();
            builder.RegisterEntryPoint<LocalizationStartup>();
            builder.Register<UIService>(Lifetime.Singleton);
            builder.RegisterBuildCallback(RegisterUiWidgets);

            return builder;
        }

        public static IContainerBuilder RegisterRootSceneUi(this IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<MainMenuWidget>();
            builder.RegisterComponentInHierarchy<MainMenuLayout>();
            builder.RegisterComponentInHierarchy<PauseMenuView>();
            builder.RegisterComponentInHierarchy<SettingsView>();
            builder.RegisterComponentInHierarchy<NavigationInputHost>();

            return builder;
        }

        private static void RegisterUiWidgets(IObjectResolver container)
        {
            var ui = container.Resolve<UIService>();
            ui.Register(container.Resolve<MainMenuWidget>());
            ui.Register(container.Resolve<PauseMenuView>());
            ui.Register(container.Resolve<SettingsView>());
        }
    }
}
