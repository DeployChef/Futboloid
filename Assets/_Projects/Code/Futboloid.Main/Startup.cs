using System;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Audio;
using Futboloid.Main.Audio;
using Futboloid.Main.DI;
using UnityEngine;
using VContainer;

namespace Futboloid.Main
{
    /// <summary>
    /// Точка входа на сцене Root.unity (единственная сцена в Build Settings).
    /// Лоадер держится, пока аудио (FMOD-банки/сэмплы) не готово — затем стартует игра.
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

            // Пока лоадер на экране — дожимаем банки/сэмплы FMOD (WebGL async).
            var audio = rootScope.Container.Resolve<IAudioManager>();
            await audio.WaitUntilReadyAsync();
            Debug.Log($"[Startup] Audio ready={audio.IsReady}");

            var director = rootScope.Container.Resolve<IGameDirector>();
            await director.InitializeGameAsync();

            // WebGL: клик по лоадеру = user gesture при живом FMOD → unlock + музыка сразу.
            // HTML «Играть» для mixerSuspend бесполезен (activation сгорает за загрузку).
            if (loadingScreen != null)
            {
                loadingScreen.blocksRaycasts = true;
                loadingScreen.interactable = true;
            }

            await WebAudioGate.WaitUntilUnlockedAsync();
            Debug.Log("[Startup] WebAudio unlocked — fade loader");

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
