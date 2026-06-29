using Futboloid.Core;
using Futboloid.Gameplay.Match;using Futboloid.Main.Navigation;
using Futboloid.Main.Session;
using VContainer;

namespace Futboloid.Main.DI
{
    public static class AppScopeExtensions
    {
        public static IContainerBuilder RegisterAppScope(this IContainerBuilder builder)
        {
            builder.RegisterInstance(GameplaySettings.Load());
            builder.Register<GameSession>(Lifetime.Singleton);
            builder.Register<TournamentRunService>(Lifetime.Singleton)
                .As<ITournamentRunService>()
                .As<ITournamentBracketReadModel>();
            builder.Register<OverlayStateController>(Lifetime.Singleton);
            builder.Register<MatchEndHandler>(Lifetime.Singleton);

            return builder;
        }
    }
}
