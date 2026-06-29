using Futboloid.Main.Navigation;
using Futboloid.Main.Session;
using VContainer;

namespace Futboloid.Main.DI
{
    public static class AppScopeExtensions
    {
        public static IContainerBuilder RegisterAppScope(this IContainerBuilder builder)
        {
            builder.Register<GameSession>(Lifetime.Singleton);
            builder.Register<OverlayStateController>(Lifetime.Singleton);

            return builder;
        }
    }
}
