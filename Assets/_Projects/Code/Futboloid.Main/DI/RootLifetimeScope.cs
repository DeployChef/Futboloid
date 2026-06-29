using Futboloid.Core;
using VContainer;
using VContainer.Unity;

namespace Futboloid.Main.DI
{
  /// <summary>
  /// Root DI scope — компонент на сцене Root.unity.
  /// Build вызывается из <see cref="Startup"/> после Awake.
  /// </summary>
  public sealed class RootLifetimeScope : LifetimeScope
  {
    protected override void Awake()
    {
      autoRun = false;
      base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
      // LifetimeScope на сцене уже регистрирует себя — RegisterInstance(this) даёт конфликт.
      builder.Register<GameDirector>(Lifetime.Singleton).As<IGameDirector>();
      builder.RegisterRootScope();
    }
  }
}
