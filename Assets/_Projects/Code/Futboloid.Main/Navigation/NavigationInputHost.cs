using Futboloid.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Futboloid.Main.Navigation
{
    /// <summary>
    /// Читает UI map (Pause) из Futboloid.inputactions на Root.
    /// </summary>
    public class NavigationInputHost : MonoBehaviour
    {
        private const string UiMapName = "UI";
        private const string PauseActionName = "Pause";

        [SerializeField] private InputActionAsset inputActions;

        private InputActionMap _uiMap;
        private InputAction _pauseAction;
        private IGameDirector _director;

        [Inject]
        public void Construct(IGameDirector director)
        {
            _director = director;
        }

        private void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError("[NavigationInputHost] InputActionAsset is not assigned.", this);
                return;
            }

            _uiMap = inputActions.FindActionMap(UiMapName, throwIfNotFound: true);
            _pauseAction = _uiMap.FindAction(PauseActionName, throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _uiMap?.Enable();
        }

        private void OnDisable()
        {
            _uiMap?.Disable();
        }

        private void Update()
        {
            if (_director == null || _pauseAction == null)
                return;

            if (!_pauseAction.WasPressedThisFrame())
                return;

            if (_director.CurrentNavigation == NavigationState.OnField)
            {
                _director.ReturnToMainMenu();
                return;
            }

            if (_director.CurrentNavigation == NavigationState.MainMenu && _director.IsMatchPausedInMenu)
                _director.GoOnField();
        }
    }
}
