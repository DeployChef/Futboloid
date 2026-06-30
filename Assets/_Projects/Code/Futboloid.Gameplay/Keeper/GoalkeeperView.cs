using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Gameplay.Ball;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Input;
using Futboloid.Gameplay.Match;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Keeper
{
    public class GoalkeeperView : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float acceleration = 40f;
        [SerializeField] private float returnToCenterSpeed = 8f;
        [SerializeField] private float centerX = 0f;
        [SerializeField] private float centerArriveThreshold = 0.02f;
        [SerializeField] private float kickoffMinX = -1.5f;
        [SerializeField] private float kickoffMaxX = 1.5f;
        [SerializeField] private float playMinX = -4.2f;
        [SerializeField] private float playMaxX = 4.2f;
        [SerializeField] private BallView ball;
        [SerializeField] private BallKickoffAnchor kickoffAnchor;

        private readonly List<IDisposable> _subscriptions = new();

        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _onField;
        private bool _returningToCenter;
        private float _velocityX;
        private IGameplayInput _input;

        [Inject]
        public void Construct(IGameEventBus bus, IGameplayInput input, PitchStateMachine pitch, MatchFlow matchFlow)
        {
            _input = input;

            if (ball == null)
                ball = FindAnyObjectByType<BallView>();

            if (kickoffAnchor == null)
                kickoffAnchor = FindAnyObjectByType<BallKickoffAnchor>();

            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            _phase = pitch.Current;
            _onField = matchFlow.IsOnField;
        }

        private void Update()
        {
            if (!_onField)
                return;

            switch (_phase)
            {
                case PitchPhase.Reshuffle:
                    TickKickoffMovement();
                    break;

                case PitchPhase.KickoffWait:
                    UpdateKickoffAim();

                    if (WasServePressed())
                    {
                        var direction = kickoffAnchor != null ? kickoffAnchor.ServeDirection : Vector2.up;
                        ball?.TryServe(direction);
                    }

                    TickKickoffMovement();
                    break;

                case PitchPhase.Simulating:
                    if (_returningToCenter)
                        _returningToCenter = false;

                    ApplyHorizontalMovement(playMinX, playMaxX);
                    break;
            }
        }

        private void TickKickoffMovement()
        {
            if (HasMoveInput())
            {
                _returningToCenter = false;
                ApplyHorizontalMovement(kickoffMinX, kickoffMaxX);
                return;
            }

            if (_returningToCenter)
                AdvanceReturnToCenter();
        }

        private void UpdateKickoffAim()
        {
            if (kickoffAnchor == null)
                return;

            var halfWidth = (kickoffMaxX - kickoffMinX) * 0.5f;
            kickoffAnchor.UpdateAimFromKeeperX(transform.position.x, halfWidth);
        }

        private void BeginReturnToCenter() => _returningToCenter = true;

        private void AdvanceReturnToCenter() => SlideHorizontalTowards(centerX);

        private void SlideHorizontalTowards(float targetX)
        {
            var position = transform.position;
            var delta = targetX - position.x;

            if (Mathf.Abs(delta) <= centerArriveThreshold)
            {
                position.x = targetX;
                transform.position = position;
                _velocityX = 0f;
                _returningToCenter = false;
                return;
            }

            var desiredVelocity = Mathf.Sign(delta) * returnToCenterSpeed;
            _velocityX = Mathf.MoveTowards(_velocityX, desiredVelocity, acceleration * Time.deltaTime);
            position.x += _velocityX * Time.deltaTime;

            if (Mathf.Sign(targetX - position.x) != Mathf.Sign(delta))
            {
                position.x = targetX;
                _velocityX = 0f;
                _returningToCenter = false;
            }

            transform.position = position;
        }

        private void ApplyHorizontalMovement(float minX, float maxX)
        {
            var position = transform.position;
            if (position.x < minX || position.x > maxX)
            {
                SlideHorizontalTowards(Mathf.Clamp(position.x, minX, maxX));
                return;
            }

            var moveX = ReadMoveX();
            var desiredVelocity = Mathf.Abs(moveX) < 0.001f ? 0f : moveX * speed;
            _velocityX = Mathf.MoveTowards(_velocityX, desiredVelocity, acceleration * Time.deltaTime);

            var previousX = position.x;
            position.x = Mathf.Clamp(position.x + _velocityX * Time.deltaTime, minX, maxX);

            if (position.x <= minX && _velocityX < 0f || position.x >= maxX && _velocityX > 0f)
                _velocityX = 0f;
            else if (Mathf.Abs(position.x - previousX) < 0.0001f && Mathf.Abs(desiredVelocity) > 0.001f)
                _velocityX = 0f;

            transform.position = position;
        }

        private float ReadMoveX() => _input?.MoveX ?? 0f;

        private bool HasMoveInput() => Mathf.Abs(ReadMoveX()) > 0.001f;

        private bool WasServePressed() =>
            _input != null && _input.WasServePressedThisFrame;

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            var previous = _phase;
            _phase = e.Phase;

            if (e.Phase == PitchPhase.KickoffWait && previous != PitchPhase.KickoffWait)
            {
                if (previous != PitchPhase.Reshuffle)
                    BeginReturnToCenter();
            }
            else if (previous == PitchPhase.Simulating
                     && e.Phase != PitchPhase.Simulating
                     && e.Phase != PitchPhase.KickoffWait)
            {
                BeginReturnToCenter();
            }
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            _onField = e.Current == NavigationState.OnField;

            if (_onField)
            {
                _returningToCenter = false;
                _velocityX = 0f;
                return;
            }

            if (e.IsMatchPausedInMenu)
            {
                _returningToCenter = false;
                return;
            }

            if (e.Previous == NavigationState.OnField)
                BeginReturnToCenter();
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
