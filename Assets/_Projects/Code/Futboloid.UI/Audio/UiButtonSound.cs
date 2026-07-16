using Futboloid.Core.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Audio
{
    /// <summary>
    /// Hover / click SFX for UI buttons. Drop on a Button; override sound ids in Inspector when needed.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class UiButtonSound : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private bool playHover = true;
        [SerializeField] private bool playClick = true;
        [SerializeField] private string hoverSoundId = AudioCatalog.Ids.UiMove;
        [SerializeField] private string clickSoundId = AudioCatalog.Ids.UiClick;

        private Button _button;
        private IAudioManager _audio;

        [Inject]
        public void Construct(IAudioManager audio)
        {
            _audio = audio;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (playClick)
                _button.onClick.AddListener(PlayClick);
        }

        private void OnDestroy()
        {
            if (_button != null && playClick)
                _button.onClick.RemoveListener(PlayClick);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!playHover || !IsButtonReady())
                return;

            Play(hoverSoundId);
        }

        private void PlayClick()
        {
            if (!playClick || !IsButtonReady())
                return;

            Play(clickSoundId);
        }

        private bool IsButtonReady()
        {
            return isActiveAndEnabled && _button != null && _button.interactable;
        }

        private void Play(string soundId)
        {
            if (_audio == null || string.IsNullOrEmpty(soundId))
                return;

            _audio.Play(soundId);
        }
    }
}
