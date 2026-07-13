using System.Collections.Generic;
using Futboloid.Core.Audio;
using Futboloid.Core.Localization;
using Futboloid.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.Settings
{
    public class SettingsView : MonoBehaviour, IWidget
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Dropdown localeDropdown;

        [Header("Volume")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        private ILocalizationService _localization;
        private IAudioManager _audio;
        private readonly List<LocaleOption> _localeOptions = new();
        private bool _isUpdatingDropdown;

        [Inject]
        public void Construct(ILocalizationService localization, IAudioManager audio)
        {
            _localization = localization;
            _audio = audio;
        }

        private void Awake()
        {
            gameObject.SetActive(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (localeDropdown != null)
                localeDropdown.onValueChanged.AddListener(OnLocaleDropdownChanged);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        private void OnEnable()
        {
            if (_localization != null)
                _localization.LocaleChanged += Refresh;
        }

        private void OnDisable()
        {
            if (_localization != null)
                _localization.LocaleChanged -= Refresh;
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);

            if (localeDropdown != null)
                localeDropdown.onValueChanged.RemoveListener(OnLocaleDropdownChanged);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        public void Close() => gameObject.SetActive(false);

        private void Refresh()
        {
            RefreshLocaleDropdown();
            RefreshVolumeSliders();
        }

        private void RefreshLocaleDropdown()
        {
            if (_localization == null || localeDropdown == null)
                return;

            _isUpdatingDropdown = true;

            // Заполняем dropdown опциями один раз
            if (localeDropdown.options.Count == 0)
            {
                _localeOptions.Clear();
                var options = new List<TMP_Dropdown.OptionData>();

                foreach (var locale in _localization.AvailableLocales)
                {
                    _localeOptions.Add(locale);
                    options.Add(new TMP_Dropdown.OptionData(locale.DisplayName));
                }

                localeDropdown.options = options;
            }

            // Выбираем текущую локаль
            var currentCode = _localization.CurrentLocaleCode;
            for (var i = 0; i < _localeOptions.Count; i++)
            {
                if (_localeOptions[i].Code == currentCode)
                {
                    localeDropdown.SetValueWithoutNotify(i);
                    break;
                }
            }

            _isUpdatingDropdown = false;
        }

        private void OnLocaleDropdownChanged(int index)
        {
            if (_isUpdatingDropdown)
                return;

            if (index >= 0 && index < _localeOptions.Count)
                _localization?.SetLocale(_localeOptions[index].Code);
        }

        private void RefreshVolumeSliders()
        {
            if (_audio == null)
                return;

            if (musicVolumeSlider != null)
                musicVolumeSlider.SetValueWithoutNotify(_audio.GetMusicVolume());

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.SetValueWithoutNotify(_audio.GetSfxVolume());
        }

        private void OnMusicVolumeChanged(float value)
        {
            _audio?.SetMusicVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            _audio?.SetSfxVolume(value);
        }
    }
}
