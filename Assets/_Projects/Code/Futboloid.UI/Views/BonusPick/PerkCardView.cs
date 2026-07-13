using System;
using DG.Tweening;
using Futboloid.Core.Localization;
using Futboloid.Core.Run;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

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
        [Header("Selection")]
        [SerializeField] private float selectedScaleMultiplier = 1.08f;
        [SerializeField] private float selectDuration = 0.2f;
        [SerializeField] private Ease selectEase = Ease.OutBack;

        public string PerkId { get; private set; }
        public bool IsOffered => !string.IsNullOrEmpty(PerkId);

        public event Action Clicked;
        public event Action PointerEntered;

        private Tween _appearTween;
        private Tween _selectTween;
        private Vector3 _defaultScale;
        private bool _selected;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization;
        }

        private void Awake()
        {
            _defaultScale = transform.localScale;

            if (button != null)
            {
                button.onClick.AddListener(() => Clicked?.Invoke());
                SetupPointerEnter(button.gameObject);
            }
            else
            {
                SetupPointerEnter(gameObject);
            }
        }

        private void SetupPointerEnter(GameObject target)
        {
            var trigger = target.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = target.AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener(_ =>
            {
                if (IsOffered)
                    PointerEntered?.Invoke();
            });
            trigger.triggers.Add(entry);
        }

        public void Show(PerkDefinition perk, int levelAfterPick)
        {
            PerkId = perk.Id;
            _selected = false;
            SetVisible(true);

            SetSprite(cardFrameImage, perk.CardFrame);
            SetSprite(iconImage, perk.Icon);

            // Показываем только имя
            if (titleText != null)
                titleText.text = _localization.GetPerkName(perk.Id);
            
            // Показываем уровень отдельно
            if (PerkLevelText != null)
            {
                PerkLevelText.text = _localization.Get(
                    LocalizationTables.UI,
                    LocalizationKeys.PerkLevelShort,
                    levelAfterPick);
            }

            if (descriptionText != null)
                descriptionText.text = _localization.GetPerkDescription(perk.Id);

            PlayAppearAnimation();
        }

        public void SetSelected(bool selected)
        {
            if (!IsOffered)
                return;

            _selected = selected;
            _appearTween?.Kill();
            _selectTween?.Kill();

            var target = selected
                ? _defaultScale * selectedScaleMultiplier
                : _defaultScale;

            _selectTween = transform
                .DOScale(target, selectDuration)
                .SetEase(selectEase)
                .SetUpdate(true);
        }

        private void PlayAppearAnimation()
        {
            _appearTween?.Kill();

            transform.localScale = Vector3.zero;

            _appearTween = transform
                .DOScale(_selected ? _defaultScale * selectedScaleMultiplier : _defaultScale, appearDuration)
                .SetEase(appearEase)
                .SetDelay(appearDelay)
                .SetUpdate(true);
        }

        public void Hide()
        {
            _appearTween?.Kill();
            _selectTween?.Kill();
            _selected = false;
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
            _selectTween?.Kill();
        }
    }
}
