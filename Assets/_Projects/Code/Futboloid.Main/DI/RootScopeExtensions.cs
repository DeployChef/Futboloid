using VContainer;

namespace Futboloid.Main.DI
{
  public static class RootScopeExtensions
  {
    public static IContainerBuilder RegisterRootScope(this IContainerBuilder builder)
    {
      // Сюда позже: AudioService, UIService, ISaveStorage…
      return builder;
    }
  }
}
