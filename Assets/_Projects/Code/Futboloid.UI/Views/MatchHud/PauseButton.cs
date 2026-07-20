using Futboloid.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.MatchHud
{
    /// <summary>
    /// Wires a gameplay HUD button to open the pause menu — same as pressing Escape on field.
    /// Drop on a Button in the Game scene.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class PauseButton : MonoBehaviour
    {
        private Button _button;
        private IGameDirector _director;

        [Inject]
        public void Construct(IGameDirector director)
        {
            _director = director;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (_director == null)
                return;

            if (_director.CurrentNavigation == NavigationState.OnField)
                _director.ReturnToPause();
        }
    }
}
