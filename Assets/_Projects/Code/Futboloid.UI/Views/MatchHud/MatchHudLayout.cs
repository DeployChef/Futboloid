using UnityEngine;
using UnityEngine.UI;

namespace Futboloid.UI.Views.MatchHud
{
    public class MatchHudLayout : MonoBehaviour
    {
        [SerializeField] private Slider timerSlider;
        [SerializeField] private Text remainingTimeText;
        [SerializeField] private Text playerScoreText;
        [SerializeField] private Text opponentScoreText;

        private void Awake()
        {
            if (timerSlider != null)
                timerSlider.interactable = false;
        }

        public void SetTimer(float normalized, float remainingSeconds)
        {
            if (timerSlider != null)
                timerSlider.value = normalized;

            if (remainingTimeText != null)
                remainingTimeText.text = Mathf.CeilToInt(remainingSeconds).ToString();
        }

        public void SetScore(int playerScore, int opponentScore)
        {
            if (playerScoreText != null)
                playerScoreText.text = playerScore.ToString();

            if (opponentScoreText != null)
                opponentScoreText.text = opponentScore.ToString();
        }
    }
}
