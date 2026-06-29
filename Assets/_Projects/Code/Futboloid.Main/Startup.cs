using Futboloid.Core;
using UnityEngine;

namespace Futboloid.Main
{
    public sealed class Startup : MonoBehaviour
    {
        static bool started;
        static GameDirector director;

        void Awake()
        {
            if (started)
            {
                Debug.LogWarning("[Startup] Already initialized — skipping duplicate Awake.");
                return;
            }

            started = true;
            director = new GameDirector();
            director.InitializeGame();

            Application.quitting += OnApplicationQuitting;
        }

        static void OnApplicationQuitting()
        {
            director = null;
            started = false;
            Application.quitting -= OnApplicationQuitting;
        }
    }
}
