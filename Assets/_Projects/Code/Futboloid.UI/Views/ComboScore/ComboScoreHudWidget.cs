using System;
using System.Collections.Generic;
using DG.Tweening;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using TMPro;
using UnityEngine;
using VContainer;

namespace Futboloid.UI.Views.ComboScore
{
    /// <summary>
    /// HUD комбо-очков и множителя на Canvas Game.
    /// </summary>
    public class ComboScoreHudWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI multiplierText;

        [Header("Score tween")]
        [SerializeField] private float scorePunchScale = 0.2f;
        [SerializeField] private float scorePunchDuration = 0.28f;
        [SerializeField] private int scorePunchVibrato = 8;

        [Header("Multiplier tween")]
        [SerializeField] private float multiplierPunchScale = 0.35f;
        [SerializeField] private float multiplierPunchDuration = 0.32f;
        [SerializeField] private Color multiplierBoostColor = new(1f, 0.85f, 0.2f);
        [SerializeField] private float multiplierColorDuration = 0.18f;

        private readonly List<IDisposable> _subscriptions = new();

        private Color _multiplierDefaultColor = Color.white;
        private Tween _scoreTween;
        private Tween _multiplierTween;
        private Tween _multiplierColorTween;

        private void Awake()
        {
            if (multiplierText != null)
                _multiplierDefaultColor = multiplierText.color;
        }

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
            _subscriptions.Add(bus.Subscribe<ComboScoreChangedEvent>(OnComboScoreChanged));

            ApplyDisplay(0, 1);
            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            KillTweens();
        }

        private void OnNavigationChanged(NavigationChangedEvent e) =>
            gameObject.SetActive(e.Current != NavigationState.Tournament);

        private void OnComboScoreChanged(ComboScoreChangedEvent e) => ApplyDisplay(e);

        private void ApplyDisplay(ComboScoreChangedEvent e)
        {
            if (scoreText != null)
            {
                scoreText.text = e.TotalScore.ToString();

                if (e.DeltaPoints > 0)
                    PlayScorePunch(scoreText.transform);
            }

            if (multiplierText != null)
            {
                multiplierText.text = $"x{e.Multiplier}";

                if (e.Multiplier > e.PreviousMultiplier)
                    PlayMultiplierBoost(multiplierText);
                else if (e.Multiplier < e.PreviousMultiplier)
                    PlayMultiplierReset(multiplierText);
            }
        }

        private void ApplyDisplay(int totalScore, int multiplier)
        {
            if (scoreText != null)
                scoreText.text = totalScore.ToString();

            if (multiplierText != null)
                multiplierText.text = $"x{multiplier}";
        }

        private void PlayScorePunch(Transform target)
        {
            _scoreTween?.Kill();
            target.localScale = Vector3.one;
            _scoreTween = target
                .DOPunchScale(Vector3.one * scorePunchScale, scorePunchDuration, scorePunchVibrato)
                .SetUpdate(true);
        }

        private void PlayMultiplierBoost(TextMeshProUGUI text)
        {
            _multiplierTween?.Kill();
            _multiplierColorTween?.Kill();

            text.transform.localScale = Vector3.one;
            _multiplierTween = text.transform
                .DOPunchScale(Vector3.one * multiplierPunchScale, multiplierPunchDuration, 6)
                .SetUpdate(true);

            text.color = multiplierBoostColor;
            var captured = text;
            _multiplierColorTween = DOTween.To(
                    () => captured.color,
                    c => captured.color = c,
                    _multiplierDefaultColor,
                    multiplierColorDuration)
                .SetUpdate(true);
        }

        private void PlayMultiplierReset(TextMeshProUGUI text)
        {
            _multiplierTween?.Kill();
            _multiplierColorTween?.Kill();

            text.transform.localScale = Vector3.one;
            _multiplierTween = text.transform
                .DOScale(0.85f, 0.08f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (text != null)
                        text.transform.DOScale(1f, 0.12f).SetUpdate(true);
                });

            text.color = _multiplierDefaultColor;
        }

        private void KillTweens()
        {
            _scoreTween?.Kill();
            _multiplierTween?.Kill();
            _multiplierColorTween?.Kill();
        }
    }
}
