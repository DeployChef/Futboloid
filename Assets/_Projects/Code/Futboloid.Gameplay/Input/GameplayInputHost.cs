using UnityEngine;
using UnityEngine.InputSystem;

namespace Futboloid.Gameplay.Input
{
    /// <summary>
    /// Единственная точка чтения Gameplay map из Futboloid.inputactions.
    /// Вешается на сцену Game, в Inspector — ссылка на asset.
    /// </summary>
    public class GameplayInputHost : MonoBehaviour, IGameplayInput
    {
        private const string GameplayMapName = "Gameplay";
        private const string MoveActionName = "Move";
        private const string ServeActionName = "Serve";

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private MobileGameplayControls mobileControls;

        private InputActionMap _gameplayMap;
        private InputAction _moveAction;
        private InputAction _serveAction;

        public float MoveX
        {
            get
            {
                var keyboardX = _moveAction?.ReadValue<Vector2>().x ?? 0f;
                var touchX = mobileControls != null && mobileControls.IsActive
                    ? mobileControls.MoveX
                    : 0f;

                if (Mathf.Abs(keyboardX) > 0.01f)
                    return Mathf.Clamp(keyboardX, -1f, 1f);

                return touchX;
            }
        }

        public bool WasServePressedThisFrame
        {
            get
            {
                var keyboardServe = _serveAction != null && _serveAction.WasPressedThisFrame();
                var touchServe = mobileControls != null && mobileControls.ConsumeServePressed();
                return keyboardServe || touchServe;
            }
        }

        private void Awake()
        {
            if (mobileControls == null)
                mobileControls = GetComponent<MobileGameplayControls>();

            if (mobileControls == null)
                mobileControls = gameObject.AddComponent<MobileGameplayControls>();

            if (inputActions == null)
            {
                Debug.LogError("[GameplayInputHost] InputActionAsset is not assigned.", this);
                return;
            }

            _gameplayMap = inputActions.FindActionMap(GameplayMapName, throwIfNotFound: true);
            _moveAction = _gameplayMap.FindAction(MoveActionName, throwIfNotFound: true);
            _serveAction = _gameplayMap.FindAction(ServeActionName, throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _gameplayMap?.Enable();
        }

        private void OnDisable()
        {
            _gameplayMap?.Disable();
        }
    }
}
