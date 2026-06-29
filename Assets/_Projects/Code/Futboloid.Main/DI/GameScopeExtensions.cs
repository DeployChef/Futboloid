using Futboloid.Gameplay.Match;
using VContainer;

namespace Futboloid.Main.DI
{
    public static class GameScopeExtensions
    {
        public static IContainerBuilder RegisterGameScope(this IContainerBuilder builder)
        {
            builder.Register<MatchFlow>(Lifetime.Singleton);
            builder.Register<PitchStateMachine>(Lifetime.Singleton);

            // Eager init: без Resolve конструктор не вызывается и шина молчит.
            builder.RegisterBuildCallback(resolver => resolver.Resolve<PitchStateMachine>());

            return builder;
        }
    }
}