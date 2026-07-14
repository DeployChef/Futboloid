using System;
using System.Collections.Generic;

namespace Futboloid.Core.Localization
{
    public readonly struct LocaleOption
    {
        public string Code { get; }
        public string DisplayName { get; }

        public LocaleOption(string code, string displayName)
        {
            Code = code;
            DisplayName = displayName;
        }
    }

    public interface ILocalizationService
    {
        event Action LocaleChanged;

        bool IsReady { get; }
        string CurrentLocaleCode { get; }
        IReadOnlyList<LocaleOption> AvailableLocales { get; }

        string Get(string table, string key);
        string Get(string table, string key, params object[] args);
        void SetLocale(string localeCode);
    }
}
