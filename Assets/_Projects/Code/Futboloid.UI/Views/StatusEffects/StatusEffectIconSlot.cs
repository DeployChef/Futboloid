using DG.Tweening;
using Futboloid.Core.Bus.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Futboloid.UI.Views.StatusEffects
{
    public class StatusEffectIconSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image ringImage;
        [SerializeField] private Color buffRingColor = new(0.35f, 0.85f, 0.45f, 1f);
        [SerializeField] private Color debuffRingColor = new(0.95f, 0.35f, 0.3f, 1f);

        private StatusEffectHudEntry _entry;
        private StatusEffectTooltipWidget _tooltip;
        private Tween _ringTween;

        public int InstanceId { get; private set; }

        public void Bind(
            StatusEffectAppliedEvent applied,
            StatusEffectTooltipWidget tooltip)
        {
            InstanceId = applied.InstanceId;
            _tooltip = tooltip;
            _entry = new StatusEffectHudEntry(
                applied.InstanceId,
                applied.EffectId,
                applied.IsDebuff);

            if (iconImage != null)
            {
                iconImage.sprite = applied.Icon;
                iconImage.enabled = applied.Icon != null;
            }

            if (ringImage != null)
            {
                ringImage.color = applied.IsDebuff ? debuffRingColor : buffRingColor;
                ringImage.fillAmount = 1f;
            }

            KillRingTween();

            if (ringImage != null && applied.DurationSeconds > 0f)
                _ringTween = TweenRingFill(1f, 0f, applied.DurationSeconds);

            gameObject.SetActive(true);
        }

        public void RefreshRing(float durationSeconds)
        {
            KillRingTween();

            if (ringImage == null || durationSeconds <= 0f)
                return;

            ringImage.fillAmount = 1f;
            _ringTween = TweenRingFill(1f, 0f, durationSeconds);
        }

        public void Hide()
        {
            KillRingTween();
            InstanceId = 0;
            gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData) =>
            _tooltip?.Show(_entry);

        public void OnPointerExit(PointerEventData eventData) =>
            _tooltip?.Hide();

        private void OnDestroy() => KillRingTween();

        private void KillRingTween()
        {
            _ringTween?.Kill();
            _ringTween = null;
        }

        private Tween TweenRingFill(float from, float to, float durationSeconds)
        {
            var image = ringImage;
            image.fillAmount = from;
            return DOTween.To(
                    () => image.fillAmount,
                    value => image.fillAmount = value,
                    to,
                    durationSeconds)
                .SetEase(Ease.Linear)
                .SetTarget(image);
        }
    }
}
