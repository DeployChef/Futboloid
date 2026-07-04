using System;
using Futboloid.Core.Run;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Futboloid.UI.Views.BonusPick
{
    /// <summary>Карточка BonusPick: frame + icon + тексты из PerkDefinition.</summary>
    public class PerkCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image cardFrameImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        public string PerkId { get; private set; }

        public event Action Clicked;

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(() => Clicked?.Invoke());
        }

        public void Show(PerkDefinition perk, int levelAfterPick)
        {
            PerkId = perk.Id;
            SetVisible(true);

            SetSprite(cardFrameImage, perk.CardFrame);
            SetSprite(iconImage, perk.Icon);

            if (titleText != null)
                titleText.text = perk.GetLevelLabel(levelAfterPick);

            if (descriptionText != null)
                descriptionText.text = perk.Description;
        }

        public void Hide()
        {
            PerkId = null;
            SetVisible(false);
        }

        private static void SetSprite(Image image, Sprite sprite)
        {
            if (image == null)
                return;

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private void SetVisible(bool visible) => gameObject.SetActive(visible);
    }
}
