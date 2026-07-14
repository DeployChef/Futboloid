using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Futboloid.Gameplay.Input
{
    /// <summary>
    /// No on-screen controls. Hold left/right half to move; short tap to serve.
    /// Prefers Touchscreen; falls back to Mouse (WebGL often maps finger → mouse).
    /// Ignores presses only over interactive UI (buttons), not decorative overlays like FirstTimeGuide.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MobileGameplayControls : MonoBehaviour
    {
        private static readonly List<RaycastResult> RaycastHits = new(8);

        [SerializeField] private float tapMaxDuration = 0.28f;
        [SerializeField] private float tapMaxMovePixels = 36f;

        private float _moveX;
        private bool _servePressedThisFrame;

        private bool _isDown;
        private bool _fromTouch;
        private bool _startedOverUi;
        private float _downTime;
        private Vector2 _downPos;

        public bool IsActive => true;

        public float MoveX => _moveX;

        public bool ConsumeServePressed()
        {
            if (!_servePressedThisFrame)
                return false;

            _servePressedThisFrame = false;
            return true;
        }

        private void Update()
        {
            _servePressedThisFrame = false;

            if (UpdateTouches())
                return;

            UpdateMouse();
        }

        private bool UpdateTouches()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            var primary = touchscreen.primaryTouch;
            var pressed = primary.press.isPressed;
            var screenPos = primary.position.ReadValue();

            if (primary.press.wasPressedThisFrame)
                Begin(screenPos, fromTouch: true);

            if (pressed)
            {
                if (_isDown && !_startedOverUi)
                    _moveX = screenPos.x < Screen.width * 0.5f ? -1f : 1f;
                else
                    _moveX = 0f;
                return true;
            }

            if (primary.press.wasReleasedThisFrame && _isDown && _fromTouch)
            {
                End(screenPos);
                return true;
            }

            return false;
        }

        private void UpdateMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                _moveX = 0f;
                return;
            }

            // Swallow the synthetic mouse click that follows a touch.
            if (_fromTouch && !_isDown)
            {
                if (mouse.leftButton.wasReleasedThisFrame || !mouse.leftButton.isPressed)
                    _fromTouch = false;
                return;
            }

            var screenPos = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame)
                Begin(screenPos, fromTouch: false);

            if (mouse.leftButton.isPressed)
            {
                if (_isDown && !_startedOverUi)
                    _moveX = screenPos.x < Screen.width * 0.5f ? -1f : 1f;
                else
                    _moveX = 0f;
                return;
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDown && !_fromTouch)
                End(screenPos);
            else if (!_isDown)
                _moveX = 0f;
        }

        private void Begin(Vector2 screenPos, bool fromTouch)
        {
            _isDown = true;
            _fromTouch = fromTouch;
            _downTime = Time.unscaledTime;
            _downPos = screenPos;
            _startedOverUi = IsOverInteractiveUi(screenPos);

            if (_startedOverUi)
            {
                _moveX = 0f;
                return;
            }

            _moveX = screenPos.x < Screen.width * 0.5f ? -1f : 1f;
        }

        private void End(Vector2 screenPos)
        {
            var duration = Time.unscaledTime - _downTime;
            var moved = Vector2.Distance(screenPos, _downPos);
            var overUi = _startedOverUi || IsOverInteractiveUi(screenPos);

            if (!overUi && duration <= tapMaxDuration && moved <= tapMaxMovePixels)
                _servePressedThisFrame = true;

            _isDown = false;
            _moveX = 0f;
            _startedOverUi = false;
        }

        private static bool IsOverInteractiveUi(Vector2 screenPos)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            var eventData = new PointerEventData(eventSystem)
            {
                position = screenPos,
            };

            RaycastHits.Clear();
            eventSystem.RaycastAll(eventData, RaycastHits);

            for (var i = 0; i < RaycastHits.Count; i++)
            {
                var go = RaycastHits[i].gameObject;
                if (go == null)
                    continue;

                // Only real controls — guide / HUD text must not eat serve taps.
                if (go.GetComponentInParent<Selectable>() != null)
                    return true;
            }

            return false;
        }
    }
}
