using Futboloid.Core;
using Futboloid.Main.DI;
using UnityEngine;
using VContainer;

namespace Futboloid.Main
{
    /// <summary>
    /// Точка входа на сцене Root.unity (единственная сцена в Build Settings).
    /// </summary>
    public class Startup : MonoBehaviour
    {
        [SerializeField] private RootLifetimeScope rootScope;

        private static bool _started;

        private void Awake()
        {
            if (_started)
            {
                Debug.LogWarning("[Startup] Already initialized — skipping duplicate Awake.");
                return;
            }

            if (rootScope == null)
            {
                Debug.LogError("[Startup] RootLifetimeScope is not assigned in the Inspector.");
                return;
            }

            _started = true;

            rootScope.Build();
            var director = rootScope.Container.Resolve<IGameDirector>();
            director.InitializeGame();

            Application.quitting += OnApplicationQuitting;
        }

        private static void OnApplicationQuitting()
        {
            _started = false;
            Application.quitting -= OnApplicationQuitting;
        }
    }
}
