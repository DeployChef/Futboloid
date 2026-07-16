using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Run;
using Futboloid.Core.StatusEffects;
using Futboloid.Gameplay.Ball;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Characters;
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
        [SerializeField] private CharacterAnimationPresenter animationPresenter;

        private readonly List<IDisposable> _subscriptions = new();
        private readonly GoalkeeperMotor _motor = new();
        private readonly GoalkeeperReshuffleMotion _reshuffle = new();

        private float _reshuffleFaceVelocityX;

        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _onField;
        private IGameplayInput _input;
        private BallView _ball;
        private PitchBounds _pitchBounds;
        private IRunProgressionService _runProgression;
        private IStatusEffectService _statusEffects;
        private Vector3 _baseLocalScale = Vector3.one;
        private bool _baseScaleCaptured;

        [Inject]
        public void Construct(
            IGameEventBus bus,
            IGameplayInput input,
            PitchStateMachine pitch,
            MatchFlow matchFlow,
            BallView ball,
            PitchBounds pitchBounds,
            IRunProgressionService runProgression,
            IStatusEffectService statusEffects)
        {
            _input = input;
            _ball = ball;
            _pitchBounds = pitchBounds;
            _runProgression = runProgression;
            _statusEffects = statusEffects;

            if (!_baseScaleCaptured)
            {
                _baseLocalScale = transform.localScale;
                _baseScaleCaptured = true;
            }

            if (kickoffAnchor == null)
                Debug.LogWarning("[GoalkeeperView] BallKickoffAnchor is not assigned.", this);

            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
            _subscriptions.Add(bus.Subscribe<PerkPickedEvent>(_ => ApplyWidthScale()));
            _subscriptions.Add(bus.Subscribe<RunProgressionUpdatedEvent>(_ => ApplyWidthScale()));
            _subscriptions.Add(bus.Subscribe<TournamentRunStartedEvent>(_ => ApplyWidthScale()));
            _subscriptions.Add(bus.Subscribe<PitchResetRequestedEvent>(_ => ApplyWidthScale()));
            _subscriptions.Add(bus.Subscribe<StatusEffectAppliedEvent>(_ => ApplyWidthScale()));
            _subscriptions.Add(bus.Subscribe<StatusEffectRemovedEvent>(_ => ApplyWidthScale()));
            _subscriptions.Add(bus.Subscribe<StatusEffectRefreshedEvent>(_ => ApplyWidthScale()));

            _phase = pitch.Current;
            _onField = matchFlow.IsOnField;
            ApplyWidthScale();
        }

        public UniTask PlayReshuffleToCenterAsync(float moveDuration, CancellationToken ct)
        {
            _motor.ResetVelocity();
            _reshuffleFaceVelocityX = centerX - transform.position.x;
            animationPresenter?.SetLocomotion(true, _reshuffleFaceVelocityX);
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
            if (!_onField || _pitchBounds == null)
                return;

            if (_reshuffle.IsActive)
            {
                animationPresenter?.SetLocomotion(true, _reshuffleFaceVelocityX);
                return;
            }

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
            var moveInput = (_input?.MoveX ?? 0f)
                * (_statusEffects?.GetMultiplier(StatId.GoalkeeperMoveInput) ?? 1f);
            var speedMultiplier =
                (_runProgression?.GetGoalkeeperMoveSpeedMultiplier() ?? 1f)
                * (_statusEffects?.GetMultiplier(StatId.GoalkeeperMoveSpeed) ?? 1f);
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
            animationPresenter?.SetLocomotion(result.IsMoving, result.VelocityX);
        }

        private void ApplyWidthScale()
        {
            var perkMul = _runProgression?.GetGoalkeeperWidthMultiplier() ?? 1f;
            var statusMul = _statusEffects?.GetMultiplier(StatId.GoalkeeperWidth) ?? 1f;
            var scale = _baseLocalScale;
            scale.x = _baseLocalScale.x * perkMul * statusMul;
            transform.localScale = scale;
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
