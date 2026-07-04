using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.MatchHud
{
    /// <summary>
    /// HUD матча на сцене Game — слайдер, счёт, видимость при уходе с поля.
    /// </summary>
    public class MatchHudWidget : MonoBehaviour
    {
        [SerializeField] private Slider timerSlider;
        [SerializeField] private TextMeshProUGUI remainingTimeText;
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI opponentScoreText;

        private readonly List<IDisposable> _subscriptions = new();

        private void Awake()
        {
            if (timerSlider != null)
                timerSlider.interactable = false;
        }

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
            _subscriptions.Add(bus.Subscribe<MatchTimerChangedEvent>(OnTimerChanged));
            _subscriptions.Add(bus.Subscribe<MatchScoreChangedEvent>(OnScoreChanged));

            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnNavigationChanged(NavigationChangedEvent e) =>
            gameObject.SetActive(e.Current != NavigationState.Tournament);

        private void OnTimerChanged(MatchTimerChangedEvent e)
        {
            if (timerSlider != null)
                timerSlider.value = e.Normalized;

            if (remainingTimeText != null)
                remainingTimeText.text = Mathf.CeilToInt(e.RemainingSeconds).ToString();
        }

        private void OnScoreChanged(MatchScoreChangedEvent e)
        {
            if (playerScoreText != null)
                playerScoreText.text = e.PlayerScore.ToString();

            if (opponentScoreText != null)
                opponentScoreText.text = e.OpponentScore.ToString();
        }
    }
}
