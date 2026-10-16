using System.Runtime.InteropServices;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Futboloid.Main.Audio
{
    /// <summary>
    /// WebGL: один раз mixerSuspend/Resume после жеста «Играть» (флаг из index.html)
    /// или после первого жеста в билде. Повторные вызовы дают спам
    /// FMOD_OS_CriticalSection_* NULL и лагают кадр — поэтому попытка строго одна.
    /// </summary>
    public sealed class WebAudioGate : MonoBehaviour
    {
        public const string GameObjectName = "WebAudioGate";

        private static WebAudioGate _instance;
        private bool _attempted;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int Futboloid_HasWebAudioGesture();

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
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        /// <summary>После готовности банков. Жест лендинга уже был — unlock один раз.</summary>
        public static void UnlockIfNeeded()
        {
            if (_instance == null || _instance._attempted || !RuntimeManager.IsInitialized)
                return;

            if (!HasLandingGesture())
                return;

            _instance.TryUnlockOnce();
        }

        private void Update()
        {
            if (_attempted || !RuntimeManager.IsInitialized)
                return;

            // Fallback только на новый жест внутри игры (не каждый кадр).
            if (!HasRuntimeGesture())
                return;

            TryUnlockOnce();
        }

        private static bool HasLandingGesture()
        {
            try
            {
                return Futboloid_HasWebAudioGesture() != 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool HasRuntimeGesture()
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
            // Сразу, до вызова FMOD — иначе при ERR_* Update будет долбить каждый жест/кадр.
            _attempted = true;

            try
            {
                var core = RuntimeManager.CoreSystem;
                if (!core.hasHandle())
                {
                    Debug.LogWarning("[WebAudioGate] CoreSystem has no handle — skip unlock");
                    return;
                }

                var suspend = core.mixerSuspend();
                var resume = core.mixerResume();
                Debug.Log($"[WebAudioGate] unlock once suspend={suspend} resume={resume}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[WebAudioGate] unlock failed: {e.Message}");
            }
        }
#else
        public static void UnlockIfNeeded()
        {
        }
#endif
    }
}
