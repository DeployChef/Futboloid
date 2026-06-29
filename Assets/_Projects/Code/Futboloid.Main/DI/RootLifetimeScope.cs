using Futboloid.Core;
using Futboloid.Main;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Main.DI
{
    /// <summary>
    /// Root DI scope на сцене Root.unity.
    /// Auto Run — выключить в Inspector; Build вызывает <see cref="Startup"/>.
    /// </summary>
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameDirector>(Lifetime.Singleton).As<IGameDirector>();
            builder.RegisterRootScope();
            builder.RegisterRootSceneUi();
        }
    }
}
