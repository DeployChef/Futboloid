using System;
using DG.Tweening;
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
        [SerializeField] private TextMeshProUGUI PerkLevelText;
        [Tooltip("Длительность анимации появления в секундах.")]
        [SerializeField] private float appearDuration = 0.35f;
        [Tooltip("Кривая плавности анимации появления.")]
        [SerializeField] private Ease appearEase = Ease.OutBack;
        [Tooltip("Задержка перед стартом анимации (для каскада карточек).")]
        [SerializeField] private float appearDelay = 0f;
        public string PerkId { get; private set; }

        public event Action Clicked;

        private Tween _appearTween;
        private Vector3 _defaultScale;

        private void Awake()
        {
            _defaultScale = transform.localScale;

            if (button != null)
                button.onClick.AddListener(() => Clicked?.Invoke());
        }

        public void Show(PerkDefinition perk, int levelAfterPick)
        {
            PerkId = perk.Id;
            SetVisible(true);

            SetSprite(cardFrameImage, perk.CardFrame);
            SetSprite(iconImage, perk.Icon);

            // Показываем только имя
            if (titleText != null)
                titleText.text = perk.DisplayName;
            
            // Показываем уровень отдельно
            if (PerkLevelText != null)
                PerkLevelText.text = $"Ур. {levelAfterPick}";

            if (descriptionText != null)
                descriptionText.text = perk.Description;

            PlayAppearAnimation();
        }

        private void PlayAppearAnimation()
        {
            _appearTween?.Kill();

            transform.localScale = Vector3.zero;

            _appearTween = transform
                .DOScale(_defaultScale, appearDuration)
                .SetEase(appearEase)
                .SetDelay(appearDelay)
                .SetUpdate(true);
        }

        public void Hide()
        {
            _appearTween?.Kill();
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

        private void OnDestroy()
        {
            _appearTween?.Kill();
        }
    }
}
