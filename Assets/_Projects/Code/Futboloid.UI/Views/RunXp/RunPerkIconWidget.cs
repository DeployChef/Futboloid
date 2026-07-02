using Futboloid.Core.Bus.Events;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Futboloid.UI.Views.RunXp
{
    /// <summary>Иконка взятого перка в Run HUD. Наведение — тултип.</summary>
    public class RunPerkIconWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image cardFrameImage;
        [SerializeField] private TextMeshProUGUI levelText;

        private RunPerkHudEntry _entry;
        private RunPerkTooltipWidget _tooltip;

        public void Bind(RunPerkHudEntry entry, RunPerkTooltipWidget tooltip)
        {
            _entry = entry;
            _tooltip = tooltip;

            if (cardFrameImage != null)
            {
                if (entry.CardFrame != null)
                    cardFrameImage.sprite = entry.CardFrame;

                cardFrameImage.enabled = true;
            }

            if (levelText != null)
                levelText.text = entry.Level.ToString();

            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        public void OnPointerEnter(PointerEventData eventData) =>
            _tooltip?.Show(_entry);

        public void OnPointerExit(PointerEventData eventData) =>
            _tooltip?.Hide();
    }
}
