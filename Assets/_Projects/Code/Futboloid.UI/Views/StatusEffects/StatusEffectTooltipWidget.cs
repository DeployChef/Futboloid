using TMPro;
using UnityEngine;

namespace Futboloid.UI.Views.StatusEffects
{
    public class StatusEffectTooltipWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private void Awake() => gameObject.SetActive(false);

        public void Show(StatusEffectHudEntry entry)
        {
            if (titleText != null)
                titleText.text = entry.Title;

            if (descriptionText != null)
                descriptionText.text = entry.Description;

            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
