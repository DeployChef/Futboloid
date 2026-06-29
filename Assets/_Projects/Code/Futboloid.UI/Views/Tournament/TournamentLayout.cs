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

        private IGameDirector _director;

        public void Bind(IGameDirector director) => _director = director;

        private void Awake()
        {
            if (matchButton != null)
                matchButton.onClick.AddListener(OnMatchClicked);
        }

        private void OnDestroy()
        {
            if (matchButton != null)
                matchButton.onClick.RemoveListener(OnMatchClicked);
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

            if (matchButton != null)
                matchButton.gameObject.SetActive(run.CanStartNextMatch);
        }

        private void OnMatchClicked() => _director?.GoOnField();
    }
}
