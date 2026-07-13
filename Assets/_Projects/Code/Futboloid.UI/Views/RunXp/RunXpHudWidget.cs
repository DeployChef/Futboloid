using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Run;
using Futboloid.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.RunXp
{
    /// <summary>
    /// HUD забега: уровень, XP, иконки взятых перков, тултип по наведению.
    /// </summary>
    public class RunXpHudWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI runLevelText;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private Transform perkIconRoot;
        [SerializeField] private RunPerkIconWidget perkIconPrefab;
        [SerializeField] private RunPerkTooltipWidget tooltip;

        private readonly List<IDisposable> _subscriptions = new();
        private readonly List<RunPerkIconWidget> _iconPool = new();

        private IRunProgressionService _progression;
        private ILocalizationService _localization;

        private void Awake()
        {
            if (xpSlider != null)
                xpSlider.interactable = false;
        }

        [Inject]
        public void Construct(
            IGameEventBus bus,
            IRunProgressionService progression,
            ILocalizationService localization)
        {
            _progression = progression;
            _localization = localization;

            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
            _subscriptions.Add(bus.Subscribe<RunProgressionUpdatedEvent>(OnProgressionUpdated));

            _localization.LocaleChanged += OnLocaleChanged;
            _progression.NotifyHud();
            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            if (_localization != null)
                _localization.LocaleChanged -= OnLocaleChanged;

            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnLocaleChanged() => _progression?.NotifyHud();

        private void OnNavigationChanged(NavigationChangedEvent e) =>
            gameObject.SetActive(e.Current != NavigationState.Tournament);

        private void OnProgressionUpdated(RunProgressionUpdatedEvent e)
        {
            if (runLevelText != null)
            {
                runLevelText.text = _localization.Get(
                    LocalizationTables.UI,
                    LocalizationKeys.RunLevelShort,
                    e.RunLevel);
            }

            if (xpSlider != null)
                xpSlider.value = e.Fill01;

            if (xpText != null)
                xpText.text = $"{e.CurrentXp} / {e.XpToNextLevel}";

            RefreshPerkIcons(e.Perks);
        }

        private void RefreshPerkIcons(RunPerkHudEntry[] perks)
        {
            if (perkIconPrefab == null || perkIconRoot == null)
            {
                if (perks.Length > 0)
                    Debug.LogWarning(
                        "[RunXpHudWidget] Perk icons not shown: assign Perk Icon Prefab and Perk Icon Root on RunXpHudWidget.");
                return;
            }

            while (_iconPool.Count < perks.Length)
            {
                var icon = Instantiate(perkIconPrefab, perkIconRoot);
                _iconPool.Add(icon);
            }

            for (var i = 0; i < _iconPool.Count; i++)
            {
                if (i < perks.Length)
                    _iconPool[i].Bind(perks[i], tooltip);
                else
                    _iconPool[i].Hide();
            }
        }
    }
}
