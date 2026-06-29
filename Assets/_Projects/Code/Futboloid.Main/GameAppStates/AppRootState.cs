using Cysharp.Threading.Tasks;
using Futboloid.Main.DI;
using UnityEngine;
using VContainer.Unity;

namespace Futboloid.Main.GameAppStates
{
  public sealed class AppRootState
  {
    public LifetimeScope RootLifetimeScope { get; }

    public AppRootState(RootLifetimeScope rootLifetimeScope)
    {
      RootLifetimeScope = rootLifetimeScope;
    }

    public UniTask Enter()
    {
      var sceneName = RootLifetimeScope.gameObject.scene.name;
      Debug.Log($"[AppRootState] Root scope ready (scene: {sceneName}).");
      return UniTask.CompletedTask;
    }
  }
}
