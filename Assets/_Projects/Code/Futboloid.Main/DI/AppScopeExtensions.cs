using VContainer;

namespace Futboloid.Main.DI
{
    public static class AppScopeExtensions
    {
        public static IContainerBuilder RegisterAppScope(this IContainerBuilder builder)
        {
            builder.Register<Session.GameSession>(Lifetime.Singleton);
            builder.Register<Navigation.OverlayStateController>(Lifetime.Singleton);

            return builder;
        }
    }
}
