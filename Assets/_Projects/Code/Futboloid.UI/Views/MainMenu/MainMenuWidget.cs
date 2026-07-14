using Futboloid.UI;
using Futboloid.UI.Views.Leaderboards;
using UnityEngine;

namespace Futboloid.UI.Views.MainMenu
{
    public class MainMenuWidget : MonoBehaviour, IWidget
    {
        [SerializeField] private MainMenuLayout layout;
        [SerializeField] private LeaderboardRefreshHub leaderboardHub;

        public void Open() => Open(false);

        public void Open(bool showContinue)
        {
            gameObject.SetActive(true);

            if (layout == null)
                layout = GetComponent<MainMenuLayout>();

            if (leaderboardHub == null)
                leaderboardHub = GetComponentInChildren<LeaderboardRefreshHub>(true);

            layout?.SetPausedMode(showContinue);
            leaderboardHub?.Refresh();
        }

        public void Close() => gameObject.SetActive(false);
    }
}
