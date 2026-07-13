using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Futboloid.Core.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Futboloid.Main.Localization
{
    public sealed class LocalizationService : ILocalizationService, IDisposable
    {
        private const string PlayerPrefsKey = "futboloid.locale";

        private bool _unityLocaleHooked;

        public event Action LocaleChanged;

        public bool IsReady { get; private set; }
        public string CurrentLocaleCode { get; private set; } = LocaleCodes.Default;

        public IReadOnlyList<LocaleOption> AvailableLocales
        {
            get
            {
                var locales = LocalizationSettings.AvailableLocales;
                if (locales == null || locales.Locales == null || locales.Locales.Count == 0)
                {
                    return new[]
                    {
                        new LocaleOption(LocaleCodes.English, "English"),
                        new LocaleOption(LocaleCodes.Russian, "Русский"),
                    };
                }

                return locales.Locales
                    .Select(locale => new LocaleOption(locale.Identifier.Code, locale.LocaleName))
                    .ToArray();
            }
        }

        public async UniTask InitializeAsync()
        {
            if (IsReady)
                return;

            await LocalizationSettings.InitializationOperation.ToUniTask();

            if (!_unityLocaleHooked)
            {
                LocalizationSettings.SelectedLocaleChanged += OnUnitySelectedLocaleChanged;
                _unityLocaleHooked = true;
            }

            var savedCode = PlayerPrefs.GetString(PlayerPrefsKey, LocaleCodes.Default);
            if (!TryApplyLocale(savedCode))
                TryApplyLocale(LocaleCodes.Default);

            IsReady = true;
        }

        public string Get(string table, string key)
        {
            if (!IsReady)
                return key;

            var localized = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
            return string.IsNullOrEmpty(localized) ? key : localized;
        }

        public string Get(string table, string key, params object[] args)
        {
            if (!IsReady)
                return key;

            var localized = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
            return string.IsNullOrEmpty(localized) ? key : localized;
        }

        public void SetLocale(string localeCode)
        {
            if (!IsReady)
            {
                Debug.LogWarning(
                    $"[LocalizationService] SetLocale('{localeCode}') called before initialization.");
                return;
            }

            if (!TryApplyLocale(localeCode))
            {
                Debug.LogWarning(
                    $"[LocalizationService] Locale '{localeCode}' is not available.");
            }
        }

        public void Dispose()
        {
            if (_unityLocaleHooked)
            {
                LocalizationSettings.SelectedLocaleChanged -= OnUnitySelectedLocaleChanged;
                _unityLocaleHooked = false;
            }
        }

        private bool TryApplyLocale(string localeCode)
        {
            var locales = LocalizationSettings.AvailableLocales?.Locales;
            if (locales == null || locales.Count == 0)
            {
                CurrentLocaleCode = localeCode;
                PlayerPrefs.SetString(PlayerPrefsKey, localeCode);
                PlayerPrefs.Save();
                return true;
            }

            var locale = locales.FirstOrDefault(
                candidate => candidate.Identifier.Code == localeCode);

            if (locale == null)
                return false;

            if (LocalizationSettings.SelectedLocale == locale)
            {
                CurrentLocaleCode = localeCode;
                PlayerPrefs.SetString(PlayerPrefsKey, localeCode);
                PlayerPrefs.Save();
                return true;
            }

            LocalizationSettings.SelectedLocale = locale;
            return true;
        }

        private void OnUnitySelectedLocaleChanged(UnityEngine.Localization.Locale locale)
        {
            if (locale == null)
                return;

            var code = locale.Identifier.Code;
            if (CurrentLocaleCode == code)
                return;

            CurrentLocaleCode = code;
            PlayerPrefs.SetString(PlayerPrefsKey, code);
            PlayerPrefs.Save();
            LocaleChanged?.Invoke();
        }
    }
}
