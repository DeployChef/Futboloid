using Futboloid.Core.Analytics;
using UnityEngine;
using VContainer;

namespace Futboloid.Main.DI
{
    public static class AnalyticsScopeExtensions
    {
        public static IContainerBuilder RegisterAnalytics(this IContainerBuilder builder)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            builder.Register<IAnalyticsService, DebugAnalyticsService>(Lifetime.Singleton);
#else
            builder.Register<IAnalyticsService, NullAnalyticsService>(Lifetime.Singleton);
#endif
            builder.Register<AnalyticsEventBridge>(Lifetime.Singleton);
            builder.RegisterBuildCallback(resolver => resolver.Resolve<AnalyticsEventBridge>());
            return builder;
        }
    }
}
