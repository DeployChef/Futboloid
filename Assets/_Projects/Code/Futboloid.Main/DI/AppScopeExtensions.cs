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
            builder.Register<TournamentRunService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<OverlayStateController>(Lifetime.Singleton);
            builder.Register<MatchEndHandler>(Lifetime.Singleton);

            return builder;
        }
    }
}
