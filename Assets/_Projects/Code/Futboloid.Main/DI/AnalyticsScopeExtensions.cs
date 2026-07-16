using Futboloid.Core.Analytics;
using Futboloid.Main.Analytics;
using VContainer;

namespace Futboloid.Main.DI
{
    public static class AnalyticsScopeExtensions
    {
        public static IContainerBuilder RegisterAnalytics(this IContainerBuilder builder)
        {
            builder.Register<DebugAnalyticsService>(Lifetime.Singleton);
            builder.Register<UgsAnalyticsService>(Lifetime.Singleton);

            builder.Register<IAnalyticsService>(
                resolver =>
                {
                    var ugs = resolver.Resolve<UgsAnalyticsService>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    return new CompositeAnalyticsService(
                        resolver.Resolve<DebugAnalyticsService>(),
                        ugs);
#else
                    return ugs;
#endif
                },
                Lifetime.Singleton);

            builder.Register<AnalyticsEventBridge>(Lifetime.Singleton);
            builder.RegisterBuildCallback(resolver => resolver.Resolve<AnalyticsEventBridge>());
            return builder;
        }
    }
}
