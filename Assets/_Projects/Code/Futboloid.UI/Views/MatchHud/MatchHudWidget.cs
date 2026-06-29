using Futboloid.UI;
using UnityEngine;

namespace Futboloid.UI.Views.MatchHud
{
    public class MatchHudWidget : MonoBehaviour
    {
        [SerializeField] private MatchHudLayout layout;

        public void Open()
        {
            gameObject.SetActive(true);

            if (layout == null)
                layout = GetComponent<MatchHudLayout>();
        }

        public void Close() => gameObject.SetActive(false);

        public void SetTimer(float normalized, float remainingSeconds)
        {
            if (layout == null)
                layout = GetComponent<MatchHudLayout>();

            layout?.SetTimer(normalized, remainingSeconds);
        }

        public void SetScore(int playerScore, int opponentScore)
        {
            if (layout == null)
                layout = GetComponent<MatchHudLayout>();

            layout?.SetScore(playerScore, opponentScore);
        }
    }
}
