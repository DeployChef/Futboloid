using Futboloid.UI;
using UnityEngine;

namespace Futboloid.UI.Views.MainMenu
{
    public class MainMenuWidget : MonoBehaviour, IWidget
    {
        public void Open() => gameObject.SetActive(true);

        public void Close() => gameObject.SetActive(false);
    }
}
