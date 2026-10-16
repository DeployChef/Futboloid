using System.Runtime.InteropServices;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Futboloid.Main.Audio
{
    /// <summary>
    /// WebGL: снимает mute браузера (FMOD mixerSuspend/Resume).
    /// Жест «Играть» на лендинге помечается из index.html — unlock сразу после Init,
    /// без второго клика и без UnityEngine.Input (проект на Input System).
    /// </summary>
    public sealed class WebAudioGate : MonoBehaviour
    {
        public const string GameObjectName = "WebAudioGate";

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

        private void Update()
        {
            if (_unlocked || !RuntimeManager.IsInitialized)
                return;

            // 1) Клик «Играть» на лендинге (до загрузки Unity) — как itch.
            // 2) Иначе любой жест уже внутри билдa (Input System, не legacy Input).
            if (!HasLandingGesture() && !HasRuntimeGesture())
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
                core.mixerSuspend();
                core.mixerResume();
                _unlocked = true;
                Debug.Log("[WebAudioGate] unlocked");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[WebAudioGate] unlock failed: {e.Message}");
            }
        }
#endif
    }
}
