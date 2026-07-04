using Futboloid.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.MainMenu
{
    public class MainMenuLayout : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button continueButton;

        private IGameDirector _director;

        [Inject]
        public void Construct(IGameDirector director)
        {
            _director = director;
        }

        private void Awake()
        {
            playButton.onClick.AddListener(OnPlayClicked);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            SetPausedMode(false);
        }

        private void OnDestroy()
        {
            playButton.onClick.RemoveListener(OnPlayClicked);

            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        public void SetPausedMode(bool isPausedFromMatch)
        {
            if (continueButton != null)
                continueButton.gameObject.SetActive(isPausedFromMatch);

            playButton.gameObject.SetActive(!isPausedFromMatch);
        }

        private void OnPlayClicked()
        {
            _director.GoOnField();
        }

        private void OnContinueClicked()
        {
            _director.GoOnField();
        }
    }
}
