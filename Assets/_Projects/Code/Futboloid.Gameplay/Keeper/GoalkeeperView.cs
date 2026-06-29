using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Bus.Events;
using Futboloid.Gameplay.Input;
using Futboloid.Gameplay.Match;
using Futboloid.Gameplay.Scene;
using UnityEngine;

namespace Futboloid.Gameplay.Keeper
{
    public class GoalkeeperView : MonoBehaviour, IGameSceneInitializable, IGameplayInputConsumer
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float kickoffMinX = -1.5f;
        [SerializeField] private float kickoffMaxX = 1.5f;
        [SerializeField] private float playMinX = -4.2f;
        [SerializeField] private float playMaxX = 4.2f;
        [SerializeField] private BallView ball;
        [SerializeField] private BallKickoffAnchor kickoffAnchor;

        private readonly List<IDisposable> _subscriptions = new();

        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _onField;
        private IGameplayInput _input;

        public void BindInput(IGameplayInput input)
        {
            _input = input;
        }

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

            switch (_phase)
            {
                case PitchPhase.KickoffWait:
                    ApplyHorizontalMovement(kickoffMinX, kickoffMaxX);
                    UpdateKickoffAim();

                    if (WasServePressed())
                    {
                        var direction = kickoffAnchor != null ? kickoffAnchor.ServeDirection : Vector2.up;
                        ball?.TryServe(direction);
                    }

                    break;

                case PitchPhase.Simulating:
                    ApplyHorizontalMovement(playMinX, playMaxX);
                    break;
            }
        }

        private void UpdateKickoffAim()
        {
            if (kickoffAnchor == null)
                return;

            var halfWidth = (kickoffMaxX - kickoffMinX) * 0.5f;
            kickoffAnchor.UpdateAimFromKeeperX(transform.position.x, halfWidth);
        }

        private void ApplyHorizontalMovement(float minX, float maxX)
        {
            var moveX = ReadMoveX();
            if (Mathf.Abs(moveX) < 0.001f)
                return;

            var position = transform.position;
            position.x = Mathf.Clamp(
                position.x + moveX * speed * Time.deltaTime,
                minX,
                maxX);
            transform.position = position;
        }

        private float ReadMoveX() => _input?.MoveX ?? 0f;

        private bool WasServePressed() =>
            _input != null && _input.WasServePressedThisFrame;

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
