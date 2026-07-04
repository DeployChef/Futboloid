using Futboloid.UI;
using UnityEngine;

namespace Futboloid.UI.Views.MainMenu
{
    public class MainMenuWidget : MonoBehaviour, IWidget
    {
        [SerializeField] private MainMenuLayout layout;

        public void Open() => Open(false);

        public void Open(bool showContinue)
        {
            gameObject.SetActive(true);

            if (layout == null)
                layout = GetComponent<MainMenuLayout>();

            layout?.SetPausedMode(showContinue);
        }

        public void Close() => gameObject.SetActive(false);
    }
}
