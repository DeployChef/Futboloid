using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Futboloid.Core.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Futboloid.Main.Localization
{
    public sealed class LocalizationService : ILocalizationService, IDisposable
    {
        private const string PlayerPrefsKey = "futboloid.locale";

        private static readonly TableReference[] PreloadTables =
        {
            LocalizationTables.UI,
            LocalizationTables.Tournament,
            LocalizationTables.Perks,
            LocalizationTables.StatusEffects,
            LocalizationTables.Settings,
        };

        private bool _unityLocaleHooked;
        private bool _isReloadingLocale;
        private UniTask? _initializeTask;

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

        public UniTask InitializeAsync()
        {
            if (IsReady)
                return UniTask.CompletedTask;

            _initializeTask ??= InitializeInternalAsync();
            return _initializeTask.Value;
        }

        public string Get(string table, string key)
        {
            if (!IsReady)
                return key;

            try
            {
                var localized = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
                return string.IsNullOrEmpty(localized) ? key : localized;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LocalizationService] Get('{table}', '{key}') failed: {ex.Message}");
                return key;
            }
        }

        public string Get(string table, string key, params object[] args)
        {
            if (!IsReady)
                return key;

            try
            {
                var localized = LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
                return string.IsNullOrEmpty(localized) ? key : localized;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LocalizationService] Get('{table}', '{key}', args) failed: {ex.Message}");
                return key;
            }
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

        private async UniTask InitializeInternalAsync()
        {
            await LocalizationSettings.InitializationOperation.ToUniTask();

            var savedCode = PlayerPrefs.GetString(PlayerPrefsKey, LocaleCodes.Default);
            if (!TryApplyLocale(savedCode))
                TryApplyLocale(LocaleCodes.Default);

            await PreloadStringTablesAsync();

            if (!_unityLocaleHooked)
            {
                LocalizationSettings.SelectedLocaleChanged += OnUnitySelectedLocaleChanged;
                _unityLocaleHooked = true;
            }

            IsReady = true;
        }

        private async UniTask PreloadStringTablesAsync()
        {
            var handle = LocalizationSettings.StringDatabase.PreloadTables(PreloadTables);
            if (!handle.IsValid())
                return;

            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogWarning(
                    $"[LocalizationService] PreloadTables failed: {handle.OperationException?.Message}");
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

            CurrentLocaleCode = localeCode;
            PlayerPrefs.SetString(PlayerPrefsKey, localeCode);
            PlayerPrefs.Save();

            if (LocalizationSettings.SelectedLocale != locale)
                LocalizationSettings.SelectedLocale = locale;

            return true;
        }

        private void OnUnitySelectedLocaleChanged(UnityEngine.Localization.Locale locale)
        {
            if (locale == null || _isReloadingLocale)
                return;

            HandleLocaleChangedAsync(locale).Forget();
        }

        private async UniTaskVoid HandleLocaleChangedAsync(UnityEngine.Localization.Locale locale)
        {
            _isReloadingLocale = true;
            IsReady = false;

            try
            {
                var code = locale.Identifier.Code;
                CurrentLocaleCode = code;
                PlayerPrefs.SetString(PlayerPrefsKey, code);
                PlayerPrefs.Save();

                // Locale change unloads string tables; reload async — never WaitForCompletion (WebGL).
                await PreloadStringTablesAsync();
                IsReady = true;
                LocaleChanged?.Invoke();
            }
            catch (Exception ex)
            {
                IsReady = true;
                Debug.LogWarning($"[LocalizationService] Locale reload failed: {ex.Message}");
            }
            finally
            {
                _isReloadingLocale = false;
            }
        }
    }
}
