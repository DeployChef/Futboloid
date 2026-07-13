using Futboloid.Core.Bus.Events;
using Futboloid.Core.Localization;
using TMPro;
using UnityEngine;
using VContainer;

namespace Futboloid.UI.Views.RunXp
{
    /// <summary>Тултип перка при наведении на иконку в Run HUD.</summary>
    public class RunPerkTooltipWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private ILocalizationService _localization;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization;
        }

        private void Awake() => gameObject.SetActive(false);

        public void Show(RunPerkHudEntry entry)
        {
            if (titleText != null)
                titleText.text = _localization.GetPerkName(entry.PerkId);

            if (levelText != null)
            {
                levelText.text = _localization.Get(
                    LocalizationTables.UI,
                    LocalizationKeys.PerkLevelLong,
                    entry.Level);
            }

            if (descriptionText != null)
                descriptionText.text = _localization.GetPerkDescription(entry.PerkId);

            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
