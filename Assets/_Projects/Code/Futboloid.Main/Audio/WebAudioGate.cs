using FMODUnity;
using UnityEngine;

namespace Futboloid.Main.Audio
{
    /// <summary>
    /// WebGL platform-glue: один раз снимает mute браузера по первому жесту в рантайме.
    /// Не про лендинг/itch UX — про то, что FMOD на WebGL ждёт mixerSuspend/Resume из жеста.
    /// </summary>
    public sealed class WebAudioGate : MonoBehaviour
    {
        public const string GameObjectName = "WebAudioGate";

        private bool _unlocked;

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

        private void Update()
        {
            if (_unlocked || !RuntimeManager.IsInitialized)
                return;

            if (!Input.anyKeyDown && !Input.GetMouseButtonDown(0) && Input.touchCount == 0)
                return;

            RuntimeManager.CoreSystem.mixerSuspend();
            RuntimeManager.CoreSystem.mixerResume();
            _unlocked = true;
            Debug.Log("[WebAudioGate] unlocked on first runtime gesture");
        }
#endif
    }
}
