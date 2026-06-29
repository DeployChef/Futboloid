using Futboloid.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.MainMenu
{
    public class MainMenuLayout : MonoBehaviour
    {
        [SerializeField] private Button playButton;

        private IGameDirector _director;

        [Inject]
        public void Construct(IGameDirector director)
        {
            _director = director;
        }

        private void Awake()
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        private void OnDestroy()
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
        }

        private void OnPlayClicked()
        {
            _director.GoOnField();
        }
    }
}
