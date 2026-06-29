using Futboloid.Core;
using VContainer;

namespace Futboloid.Main.DI
{
    public static class RootScopeExtensions
    {
        public static IContainerBuilder RegisterRootScope(
            this IContainerBuilder builder,
            IGameDirector gameDirector)
        {
            builder.RegisterInstance(gameDirector).As<IGameDirector>();

            return builder;
        }
    }
}
