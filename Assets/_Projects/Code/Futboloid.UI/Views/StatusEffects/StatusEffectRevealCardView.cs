using DG.Tweening;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.StatusEffects
{
    /// <summary>Карточка первого показа timed-эффекта: иконка, название, описание.</summary>
    public class StatusEffectRevealCardView : MonoBehaviour
    {
        [SerializeField] private Image frameImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI kindText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Color buffFrameColor = new(0.35f, 0.85f, 0.45f, 1f);
        [SerializeField] private Color debuffFrameColor = new(0.95f, 0.35f, 0.3f, 1f);
        [SerializeField] private float appearDuration = 0.35f;
        [SerializeField] private Ease appearEase = Ease.OutBack;

        private Tween _appearTween;
        private Vector3 _defaultScale;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization;
        }

        private void Awake() => _defaultScale = transform.localScale;

        public void Show(StatusEffectAppliedEvent applied)
        {
            if (frameImage != null)
                frameImage.color = applied.IsDebuff ? debuffFrameColor : buffFrameColor;

            if (kindText != null)
            {
                kindText.text = applied.IsDebuff
                    ? _localization.Get(LocalizationTables.UI, LocalizationKeys.DebuffLabel)
                    : _localization.Get(LocalizationTables.UI, LocalizationKeys.BuffLabel);
            }

            if (titleText != null)
                titleText.text = _localization.GetStatusEffectName(applied.EffectId);

            if (descriptionText != null)
                descriptionText.text = _localization.GetStatusEffectDescription(applied.EffectId);

            SetSprite(iconImage, applied.Icon);
            PlayAppearAnimation();
        }

        public void Hide()
        {
            _appearTween?.Kill();
            gameObject.SetActive(false);
        }

        private void PlayAppearAnimation()
        {
            _appearTween?.Kill();
            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;

            _appearTween = transform
                .DOScale(_defaultScale, appearDuration)
                .SetEase(appearEase)
                .SetUpdate(true);
        }

        private static void SetSprite(Image image, Sprite sprite)
        {
            if (image == null)
                return;

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private void OnDestroy() => _appearTween?.Kill();
    }
}
