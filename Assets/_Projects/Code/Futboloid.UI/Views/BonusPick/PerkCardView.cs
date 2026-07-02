using System;
using Futboloid.Core.Run;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Futboloid.UI.Views.BonusPick
{
    /// <summary>Одна карточка в BonusPick — вешается на prefab.</summary>
    public class PerkCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image frameImage;
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

        public void Show(
            PerkDefinition perk,
            int levelAfterPick,
            Sprite frame,
            Sprite icon,
            string title,
            string description)
        {
            PerkId = perk.Id;
            SetVisible(true);

            if (frameImage != null && frame != null)
                frameImage.sprite = frame;

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            if (titleText != null)
                titleText.text = title;

            if (descriptionText != null)
                descriptionText.text = description;
        }

        public void Hide()
        {
            PerkId = null;
            SetVisible(false);
        }

        private void SetVisible(bool visible) => gameObject.SetActive(visible);
    }
}
