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

        private InputActionMap _gameplayMap;
        private InputAction _moveAction;
        private InputAction _serveAction;

        public float MoveX => _moveAction?.ReadValue<Vector2>().x ?? 0f;

        public bool WasServePressedThisFrame =>
            _serveAction != null && _serveAction.WasPressedThisFrame();

        private void Awake()
        {
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
