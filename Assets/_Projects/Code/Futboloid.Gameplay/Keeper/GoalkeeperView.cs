using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Run;
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
        [SerializeField] private float centerX = 0f;
        [SerializeField] private float centerArriveThreshold = 0.02f;
        [SerializeField] private BallKickoffAnchor kickoffAnchor;
        [SerializeField] private GoalkeeperAnimationPresenter animationPresenter;

        private readonly List<IDisposable> _subscriptions = new();
        private readonly GoalkeeperMotor _motor = new();
        private readonly GoalkeeperReshuffleMotion _reshuffle = new();

        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _onField;
        private IGameplayInput _input;
        private BallView _ball;
        private PitchBounds _pitchBounds;
        private IRunProgressionService _runProgression;

        [Inject]
        public void Construct(
            IGameEventBus bus,
            IGameplayInput input,
            PitchStateMachine pitch,
            MatchFlow matchFlow,
            BallView ball,
            PitchBounds pitchBounds,
            IRunProgressionService runProgression)
        {
            _input = input;
            _ball = ball;
            _pitchBounds = pitchBounds;
            _runProgression = runProgression;

            if (kickoffAnchor == null)
                Debug.LogWarning("[GoalkeeperView] BallKickoffAnchor is not assigned.", this);

            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            _phase = pitch.Current;
            _onField = matchFlow.IsOnField;
        }

        public UniTask PlayReshuffleToCenterAsync(float moveDuration, CancellationToken ct)
        {
            _motor.ResetVelocity();
            return _reshuffle.PlayToCenterAsync(
                transform,
                centerX,
                centerArriveThreshold,
                moveDuration,
                ct);
        }

        public void KillMoveTween()
        {
            _reshuffle.Kill(transform);
            _motor.ResetVelocity();
        }

        private void Update()
        {
            if (!_onField || _reshuffle.IsActive || _pitchBounds == null)
                return;

            switch (_phase)
            {
                case PitchPhase.Reshuffle:
                case PitchPhase.KickoffWait:
                    if (_phase == PitchPhase.KickoffWait)
                    {
                        UpdateKickoffAim();

                        if (WasServePressed())
                        {
                            var direction = kickoffAnchor != null ? kickoffAnchor.ServeDirection : Vector2.up;
                            _ball?.TryServe(direction);
                        }
                    }

                    TickMovement(_pitchBounds.KickoffMinX, _pitchBounds.KickoffMaxX);
                    break;

                case PitchPhase.Simulating:
                    TickMovement(_pitchBounds.MinX, _pitchBounds.MaxX);
                    break;
            }
        }

        private void TickMovement(float minX, float maxX)
        {
            var moveInput = _input?.MoveX ?? 0f;
            var speedMultiplier = _runProgression?.GetGoalkeeperMoveSpeedMultiplier() ?? 1f;
            var result = _motor.Tick(
                transform.position,
                minX,
                maxX,
                _pitchBounds.MinY,
                _pitchBounds.MaxY,
                moveInput,
                speed,
                speedMultiplier,
                acceleration,
                Time.deltaTime);

            transform.position = result.Position;
            animationPresenter?.SetRunning(result.IsMoving);
        }

        private void UpdateKickoffAim()
        {
            if (kickoffAnchor == null)
                return;

            kickoffAnchor.UpdateAimFromKeeperX(transform.position.x, _pitchBounds.KickoffHalfWidth);
        }

        private void SnapToCenterX()
        {
            transform.position = _motor.SnapToCenterX(transform.position, centerX);
        }

        private bool WasServePressed() =>
            _input != null && _input.WasServePressedThisFrame;

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            var previous = _phase;
            _phase = e.Phase;

            if (e.Phase == PitchPhase.KickoffWait
                && previous != PitchPhase.KickoffWait
                && previous != PitchPhase.Reshuffle)
            {
                SnapToCenterX();
            }
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            _onField = e.Current == NavigationState.OnField;

            if (_onField)
                KillMoveTween();
        }

        private void OnDestroy()
        {
            KillMoveTween();

            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
