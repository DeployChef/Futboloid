using Futboloid.Core.Bus.Events;
using Futboloid.Core.Localization;
using TMPro;
using UnityEngine;
using VContainer;

namespace Futboloid.UI.Views.StatusEffects
{
    public class StatusEffectTooltipWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private ILocalizationService _localization;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization;
        }

        private void Awake() => gameObject.SetActive(false);

        public void Show(StatusEffectHudEntry entry)
        {
            if (titleText != null)
                titleText.text = _localization.GetStatusEffectName(entry.EffectId);

            if (descriptionText != null)
                descriptionText.text = _localization.GetStatusEffectDescription(entry.EffectId);

            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
