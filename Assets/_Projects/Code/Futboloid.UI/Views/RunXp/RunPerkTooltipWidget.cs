using Futboloid.Core.Bus.Events;
using TMPro;
using UnityEngine;

namespace Futboloid.UI.Views.RunXp
{
    /// <summary>Тултип перка при наведении на иконку в Run HUD.</summary>
    public class RunPerkTooltipWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private void Awake() => gameObject.SetActive(false);

        public void Show(RunPerkHudEntry entry)
        {
            if (titleText != null)
                titleText.text = entry.Title;

            if (levelText != null)
                levelText.text = $"Уровень {entry.Level}";

            if (descriptionText != null)
                descriptionText.text = entry.Description;

            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
