using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Localization;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.UI.Views.Leaderboards;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.Tournament
{
    /// <summary>
    /// Турнирный оверлей на сцене Game — раунд, статус, кнопки матча/рестарта/меню.
    /// </summary>
    public class TournamentWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roundLabel;
        [SerializeField] private TextMeshProUGUI statusLabel;
        [SerializeField] private Button matchButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private FirstTimeGuideWidget firstTimeGuide;
        [SerializeField] private PlayerNicknameControl nicknameControl;

        private readonly List<IDisposable> _subscriptions = new();
        private IGameDirector _director;
        private ILocalizationService _localization;

        private void Awake()
        {
            if (matchButton != null)
                matchButton.onClick.AddListener(OnMatchClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        [Inject]
        public void Construct(IGameEventBus bus, IGameDirector director, ILocalizationService localization)
        {
            _director = director;
            _localization = localization;

            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            _localization.LocaleChanged += OnLocaleChanged;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsMatchContinueAvailable())
                return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                OnMatchClicked();
        }

        private void OnLocaleChanged()
        {
            if (gameObject.activeSelf)
                Refresh();
        }

        private void OnDestroy()
        {
            if (_localization != null)
                _localization.LocaleChanged -= OnLocaleChanged;

            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            if (matchButton != null)
                matchButton.onClick.RemoveListener(OnMatchClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            if (e.Current == NavigationState.Tournament)
            {
                gameObject.SetActive(true);
                Refresh();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void Refresh()
        {
            var run = _director?.TournamentBracket;
            if (run == null)
                return;

            if (roundLabel != null)
                roundLabel.text = run.RoundLabel;

            if (statusLabel != null)
                statusLabel.text = run.StatusLine;

            var inProgress = run.RunState == TournamentRunState.InProgress;

            if (matchButton != null)
                matchButton.gameObject.SetActive(inProgress);

            if (restartButton != null)
                restartButton.gameObject.SetActive(!inProgress);

            if (mainMenuButton != null)
                mainMenuButton.gameObject.SetActive(!inProgress);

            var runEnded = !inProgress;

            if (nicknameControl != null)
            {
                nicknameControl.gameObject.SetActive(runEnded);
                if (runEnded)
                    nicknameControl.LoadFromStore();
            }

            firstTimeGuide?.Refresh();
        }

        private bool IsMatchContinueAvailable()
        {
            return matchButton != null
                   && matchButton.gameObject.activeInHierarchy
                   && matchButton.interactable;
        }

        private void OnMatchClicked() => _director?.GoOnField();

        private void OnRestartClicked() => _director?.RestartTournament();

        private void OnMainMenuClicked() => _director?.ReturnToMainMenu();
    }
}
