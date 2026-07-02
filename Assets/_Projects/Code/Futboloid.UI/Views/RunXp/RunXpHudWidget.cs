using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.RunXp
{
    /// <summary>
    /// Шкала XP забега на сцене Game — отдельно от Match HUD (таймер, счёт).
    /// </summary>
    public class RunXpHudWidget : MonoBehaviour
    {
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI xpText;

        private readonly List<IDisposable> _subscriptions = new();

        private void Awake()
        {
            if (xpSlider != null)
                xpSlider.interactable = false;
        }

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
            _subscriptions.Add(bus.Subscribe<RunXpChangedEvent>(OnXpChanged));

            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnNavigationChanged(NavigationChangedEvent e) =>
            gameObject.SetActive(e.Current != NavigationState.Tournament);

        private void OnXpChanged(RunXpChangedEvent e)
        {
            if (xpSlider != null)
                xpSlider.value = e.Fill01;

            if (xpText != null)
                xpText.text = $"{e.CurrentXp} / {e.XpToNextLevel}";
        }
    }
}
