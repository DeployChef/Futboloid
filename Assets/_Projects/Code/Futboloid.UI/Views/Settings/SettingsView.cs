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
        [SerializeField] private Button englishButton;
        [SerializeField] private Button russianButton;
        [SerializeField] private TextMeshProUGUI currentLanguageLabel;

        private ILocalizationService _localization;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization;
        }

        private void Awake()
        {
            gameObject.SetActive(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (englishButton != null)
                englishButton.onClick.AddListener(() => SelectLocale(LocaleCodes.English));

            if (russianButton != null)
                russianButton.onClick.AddListener(() => SelectLocale(LocaleCodes.Russian));
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

            if (englishButton != null)
                englishButton.onClick.RemoveAllListeners();

            if (russianButton != null)
                russianButton.onClick.RemoveAllListeners();
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        public void Close() => gameObject.SetActive(false);

        private void SelectLocale(string localeCode)
        {
            _localization?.SetLocale(localeCode);
            Refresh();
        }

        private void Refresh()
        {
            if (_localization == null)
                return;

            var currentCode = _localization.CurrentLocaleCode;
            SetSelected(englishButton, currentCode == LocaleCodes.English);
            SetSelected(russianButton, currentCode == LocaleCodes.Russian);

            if (currentLanguageLabel == null)
                return;

            foreach (var option in _localization.AvailableLocales)
            {
                if (option.Code != currentCode)
                    continue;

                currentLanguageLabel.text = option.DisplayName;
                return;
            }

            currentLanguageLabel.text = currentCode;
        }

        private static void SetSelected(Button button, bool selected)
        {
            if (button == null)
                return;

            if (button.targetGraphic != null)
                button.targetGraphic.color = selected
                    ? new Color(0.85f, 1f, 0.85f, 1f)
                    : Color.white;
        }
    }
}
