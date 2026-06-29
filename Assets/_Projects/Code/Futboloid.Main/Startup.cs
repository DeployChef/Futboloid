using Futboloid.Core;
using Futboloid.Main.DI;
using UnityEngine;
using VContainer;

namespace Futboloid.Main
{
  /// <summary>
  /// Точка входа на сцене Root.unity (единственная сцена в Build Settings).
  /// </summary>
  public sealed class Startup : MonoBehaviour
  {
    [SerializeField] RootLifetimeScope rootScope;

    static bool started;

    void Awake()
    {
      if (started)
      {
        Debug.LogWarning("[Startup] Already initialized — skipping duplicate Awake.");
        return;
      }

      if (rootScope == null)
      {
        Debug.LogError("[Startup] RootLifetimeScope is not assigned in the Inspector.");
        return;
      }

      started = true;

      rootScope.Build();
      var director = rootScope.Container.Resolve<IGameDirector>();
      director.InitializeGame();

      Application.quitting += OnApplicationQuitting;
    }

    static void OnApplicationQuitting()
    {
      started = false;
      Application.quitting -= OnApplicationQuitting;
    }
  }
}
