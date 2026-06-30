using Futboloid.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Futboloid.UI.Views.Tournament
{
    public class TournamentLayout : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roundLabel;
        [SerializeField] private TextMeshProUGUI statusLabel;
        [SerializeField] private Button matchButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private IGameDirector _director;

        public void Bind(IGameDirector director) => _director = director;

        private void Awake()
        {
            if (matchButton != null)
                matchButton.onClick.AddListener(OnMatchClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDestroy()
        {
            if (matchButton != null)
                matchButton.onClick.RemoveListener(OnMatchClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        public void Refresh()
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
        }

        private void OnMatchClicked() => _director?.GoOnField();

        private void OnRestartClicked() => _director?.RestartTournament();

        private void OnMainMenuClicked() => _director?.ReturnToMainMenu();
    }
}
