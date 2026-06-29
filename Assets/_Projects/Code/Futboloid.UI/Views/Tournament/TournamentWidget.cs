using Futboloid.Core;
using UnityEngine;

namespace Futboloid.UI.Views.Tournament
{
    public class TournamentWidget : MonoBehaviour
    {
        [SerializeField] private TournamentLayout layout;

        public void BindDirector(IGameDirector director)
        {
            if (layout == null)
                layout = GetComponent<TournamentLayout>();

            layout?.Bind(director);
        }

        public void Open()
        {
            gameObject.SetActive(true);

            if (layout == null)
                layout = GetComponent<TournamentLayout>();

            layout?.Refresh();
        }

        public void Close() => gameObject.SetActive(false);
    }
}
