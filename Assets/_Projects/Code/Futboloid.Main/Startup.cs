using System;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private CanvasGroup loadingScreen;
        [SerializeField] private float loadingFadeOutSeconds = 0.45f;

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
            Application.quitting += OnApplicationQuitting;
            BootAsync().Forget();
        }

        private async UniTaskVoid BootAsync()
        {
            if (loadingScreen != null)
            {
                loadingScreen.gameObject.SetActive(true);
                loadingScreen.alpha = 1f;
                loadingScreen.blocksRaycasts = true;
            }

            rootScope.Build();
            var director = rootScope.Container.Resolve<IGameDirector>();
            await director.InitializeGameAsync();

            await FadeOutLoadingAsync();
        }

        private async UniTask FadeOutLoadingAsync()
        {
            if (loadingScreen == null)
                return;

            loadingScreen.blocksRaycasts = true;
            loadingScreen.interactable = false;

            var duration = Mathf.Max(0f, loadingFadeOutSeconds);
            if (duration <= 0f)
            {
                loadingScreen.alpha = 0f;
                loadingScreen.blocksRaycasts = false;
                loadingScreen.gameObject.SetActive(false);
                return;
            }

            var startAlpha = loadingScreen.alpha;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                loadingScreen.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            loadingScreen.alpha = 0f;
            loadingScreen.blocksRaycasts = false;
            loadingScreen.gameObject.SetActive(false);
        }

        private static void OnApplicationQuitting()
        {
            _started = false;
            Application.quitting -= OnApplicationQuitting;
        }
    }
}
