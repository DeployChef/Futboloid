using Futboloid.Core;
using Futboloid.UI;
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

        [Inject]
        public void Construct(IGameDirector director)
        {
            _director = director;
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

        private void OnSettingsClicked()
        {
            // TODO: открыть настройки
        }

        private void OnBackToMenuClicked() => _director.ReturnToMainMenu();
    }
}
