using System.Threading;
using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Futboloid.Main.Audio
{
    /// <summary>
    /// WebGL: unlock FMOD webaudio только по реальному клику/тачу в Unity
    /// (когда CoreSystem уже есть). Жест HTML «Играть» для этого бесполезен —
    /// user activation сгорает за время загрузки билда; ранний mixerSuspend
    /// ломает output → спам NULL critical section.
    /// </summary>
    public sealed class WebAudioGate : MonoBehaviour
    {
        public const string GameObjectName = "WebAudioGate";

        private static WebAudioGate _instance;
        private bool _unlocked;

        public static bool IsUnlocked =>
#if UNITY_WEBGL && !UNITY_EDITOR
            _instance != null && _instance._unlocked;
#else
            true;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (GameObject.Find(GameObjectName) != null)
                return;

            var go = new GameObject(GameObjectName);
            DontDestroyOnLoad(go);
            go.AddComponent<WebAudioGate>();
        }

        private void Awake()
        {
            _instance = this;
            RuntimeManager.AllowMixerSuspendResume = false;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void Update()
        {
            if (_unlocked || !RuntimeManager.IsInitialized)
                return;

            if (!HasPointerDown())
                return;

            TryUnlockOnce();
        }

        /// <summary>Лоадер ждёт клик: unlock → можно снимать loadingScreen.</summary>
        public static async UniTask WaitUntilUnlockedAsync(CancellationToken cancellationToken = default)
        {
            if (IsUnlocked)
                return;

            if (_instance == null)
                await UniTask.WaitUntil(() => _instance != null, cancellationToken: cancellationToken);

            await UniTask.WaitUntil(() => IsUnlocked, cancellationToken: cancellationToken);
        }

        private static bool HasPointerDown()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                return true;

            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
                return true;

            return false;
        }

        private void TryUnlockOnce()
        {
            if (_unlocked)
                return;

            _unlocked = true;
            RuntimeManager.AllowMixerSuspendResume = true;

            try
            {
                var core = RuntimeManager.CoreSystem;
                if (!core.hasHandle())
                {
                    Debug.LogWarning("[WebAudioGate] CoreSystem has no handle");
                    return;
                }

                var suspend = core.mixerSuspend();
                var resume = core.mixerResume();
                Debug.Log($"[WebAudioGate] unlocked suspend={suspend} resume={resume}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[WebAudioGate] unlock failed: {e.Message}");
            }
        }
#else
        public static UniTask WaitUntilUnlockedAsync(CancellationToken cancellationToken = default) =>
            UniTask.CompletedTask;
#endif
    }
}
