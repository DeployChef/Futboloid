using Futboloid.Core;
using Futboloid.UI;
using Futboloid.UI.Views.Settings;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.PauseMenu
{
    public class PauseMenuView : MonoBehaviour, IWidget
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button backToMenuButton;

        private IGameDirector _director;
        private UIService _ui;

        [Inject]
        public void Construct(IGameDirector director, UIService ui)
        {
            _director = director;
            _ui = ui;
        }

        private void Awake()
        {
            gameObject.SetActive(false);

            continueButton.onClick.AddListener(OnContinueClicked);
            restartButton.onClick.AddListener(OnRestartClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        private void OnDestroy()
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
            restartButton.onClick.RemoveListener(OnRestartClicked);
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
            backToMenuButton.onClick.RemoveListener(OnBackToMenuClicked);
        }

        public void Open() => gameObject.SetActive(true);

        public void Close() => gameObject.SetActive(false);

        private void OnContinueClicked() => _director.GoOnField();

        private void OnRestartClicked() => _director.RestartTournament();

        private void OnSettingsClicked() => _ui.Show<SettingsView>();

        private void OnBackToMenuClicked() => _director.ReturnToMainMenu();
    }
}
