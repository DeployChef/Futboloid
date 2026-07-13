namespace Futboloid.Core.Localization
{
    public static class LocalizationContentExtensions
    {
        public static string GetPerkName(this ILocalizationService localization, string perkId) =>
            localization.Get(LocalizationTables.Perks, LocalizationKeys.PerkName(perkId));

        public static string GetPerkDescription(this ILocalizationService localization, string perkId) =>
            localization.Get(LocalizationTables.Perks, LocalizationKeys.PerkDescription(perkId));

        public static string GetStatusEffectName(this ILocalizationService localization, string effectId) =>
            localization.Get(LocalizationTables.StatusEffects, LocalizationKeys.StatusEffectName(effectId));

        public static string GetStatusEffectDescription(
            this ILocalizationService localization,
            string effectId) =>
            localization.Get(LocalizationTables.StatusEffects, LocalizationKeys.StatusEffectDescription(effectId));
    }
}
