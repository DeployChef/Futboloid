using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Bus.Events;
using Futboloid.Gameplay.Match;
using Futboloid.Gameplay.Scene;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Futboloid.Gameplay.Keeper
{
    public class GoalkeeperView : MonoBehaviour, IGameSceneInitializable
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float kickoffMinX = -1.5f;
        [SerializeField] private float kickoffMaxX = 1.5f;
        [SerializeField] private BallView ball;
        [SerializeField] private BallKickoffAnchor kickoffAnchor;

        private readonly List<IDisposable> _subscriptions = new();

        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _onField;

        public void Initialize(IGameEventBus bus)
        {
            if (ball == null)
                ball = FindAnyObjectByType<BallView>();

            if (kickoffAnchor == null)
                kickoffAnchor = FindAnyObjectByType<BallKickoffAnchor>();

            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
        }

        private void Update()
        {
            if (!_onField)
                return;

            if (_phase == PitchPhase.KickoffWait)
            {
                ApplyKickoffMovement();

                if (WasServePressed())
                {
                    var direction = kickoffAnchor != null ? kickoffAnchor.ServeDirection : Vector2.up;
                    ball?.TryServe(direction);
                }
            }
        }

        private void ApplyKickoffMovement()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            var moveX = 0f;
            if (keyboard.aKey.isPressed)
                moveX -= 1f;
            if (keyboard.dKey.isPressed)
                moveX += 1f;

            if (Mathf.Abs(moveX) < 0.001f)
                return;

            var position = transform.position;
            position.x = Mathf.Clamp(
                position.x + moveX * speed * Time.deltaTime,
                kickoffMinX,
                kickoffMaxX);
            transform.position = position;
        }

        private static bool WasServePressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            _phase = e.Phase;
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            _onField = e.Current == NavigationState.OnField;
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
