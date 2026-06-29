using VContainer;

namespace Futboloid.Main.DI
{
  public static class AppScopeExtensions
  {
    public static IContainerBuilder RegisterAppScope(this IContainerBuilder builder)
    {
      // Позже: RootServiceLocator.Initialize, GameSession, OverlayStateController, RunStateService
      return builder;
    }
  }
}
