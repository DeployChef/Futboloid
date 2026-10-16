using System.Runtime.InteropServices;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Futboloid.Main.Audio
{
    /// <summary>
    /// WebGL: снимает mute браузера (FMOD mixerSuspend/Resume).
    /// Жест «Играть» на лендинге помечается из index.html.
    /// Unlock вызывается после готовности банков (см. FmodAudioManager.WaitUntilReadyAsync),
    /// чтобы не ловить FMOD_OS_CriticalSection_* NULL.
    /// </summary>
    public sealed class WebAudioGate : MonoBehaviour
    {
        public const string GameObjectName = "WebAudioGate";

        private static WebAudioGate _instance;
        private bool _unlocked;

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

        /// <summary>Вызвать после загрузки банков — mixerSuspend/Resume в окне жеста лендинга.</summary>
        public static void UnlockIfNeeded()
        {
            if (_instance == null || _instance._unlocked || !RuntimeManager.IsInitialized)
                return;

            if (!HasLandingGesture() && !HasRuntimeGesture())
                return;

            _instance.TryUnlock();
        }

        private void Update()
        {
            if (_unlocked || !RuntimeManager.IsInitialized)
                return;

            // Fallback: жест уже внутри игры (банки могли быть готовы раньше).
            if (!HasRuntimeGesture())
                return;

            TryUnlock();
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
            if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame))
                return true;

            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
                return true;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
                return true;

            var pen = Pen.current;
            if (pen != null && pen.tip.wasPressedThisFrame)
                return true;

            return false;
        }

        private void TryUnlock()
        {
            try
            {
                var core = RuntimeManager.CoreSystem;
                if (!core.hasHandle())
                    return;

                var result = core.mixerSuspend();
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogWarning($"[WebAudioGate] mixerSuspend: {result}");
                    return;
                }

                result = core.mixerResume();
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogWarning($"[WebAudioGate] mixerResume: {result}");
                    return;
                }

                _unlocked = true;
                Debug.Log("[WebAudioGate] unlocked");
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
