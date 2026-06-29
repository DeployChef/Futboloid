using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Match;
using VContainer;

namespace Futboloid.Main.DI
{
    public static class GameScopeExtensions
    {
        public static IContainerBuilder RegisterGameScope(this IContainerBuilder builder)
        {
            builder.Register<IGameEventBus, GameEventBus>(Lifetime.Singleton);
            builder.Register<MatchFlow>(Lifetime.Singleton);
            builder.Register<PitchStateMachine>(Lifetime.Singleton);

            return builder;
        }
    }
}
