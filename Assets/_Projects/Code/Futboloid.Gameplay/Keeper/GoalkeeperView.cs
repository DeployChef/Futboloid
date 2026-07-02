using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private float centerX = 0f;
        [SerializeField] private float centerArriveThreshold = 0.02f;
        [SerializeField] private BallKickoffAnchor kickoffAnchor;
        [SerializeField] private Animator animator;

        private readonly List<IDisposable> _subscriptions = new();

        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _onField;
        private bool _reshuffleMoving;
        private float _velocityX;
        private IGameplayInput _input;
        private BallView _ball;
        private PitchBounds _pitchBounds;
        private Tween _moveTween;

        [Inject]
        public void Construct(
            IGameEventBus bus,
            IGameplayInput input,
            PitchStateMachine pitch,
            MatchFlow matchFlow,
            BallView ball,
            PitchBounds pitchBounds)
        {
            _input = input;
            _ball = ball;
            _pitchBounds = pitchBounds;

            if (kickoffAnchor == null)
                kickoffAnchor = FindAnyObjectByType<BallKickoffAnchor>();

            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            _phase = pitch.Current;
            _onField = matchFlow.IsOnField;
        }

        public async UniTask PlayReshuffleToCenterAsync(float moveDuration, CancellationToken ct)
        {
            KillMoveTween();
            _velocityX = 0f;

            var delta = centerX - transform.position.x;
            if (Mathf.Abs(delta) <= centerArriveThreshold)
            {
                SnapToCenterX();
                return;
            }

            _reshuffleMoving = true;

            try
            {
                var distance = Mathf.Abs(delta);
                var refDistance = 4f;
                var duration = moveDuration * (distance / refDistance);
                duration = Mathf.Clamp(duration, 0.12f, 1.4f);

                _moveTween = transform
                    .DOMoveX(centerX, duration)
                    .SetEase(Ease.InOutQuad)
                    .SetLink(gameObject);

                await TweenAsync.Await(_moveTween, ct);

                if (!ct.IsCancellationRequested)
                    SnapToCenterX();
            }
            finally
            {
                _reshuffleMoving = false;
                _moveTween = null;
            }
        }

        public void KillMoveTween()
        {
            if (_moveTween != null && _moveTween.IsActive())
                _moveTween.Kill();

            _moveTween = null;
            transform.DOKill();
            _reshuffleMoving = false;
        }

        private void Start()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!_onField || _reshuffleMoving || _pitchBounds == null)
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

                    ApplyHorizontalMovement(_pitchBounds.KickoffMinX, _pitchBounds.KickoffMaxX);
                    break;

                case PitchPhase.Simulating:
                    ApplyHorizontalMovement(_pitchBounds.MinX, _pitchBounds.MaxX);
                    break;
            }
        }

        private void UpdateKickoffAim()
        {
            if (kickoffAnchor == null)
                return;

            var halfWidth = _pitchBounds != null ? _pitchBounds.KickoffHalfWidth : 1.5f;
            kickoffAnchor.UpdateAimFromKeeperX(transform.position.x, halfWidth);
        }

        private void SnapToCenterX()
        {
            var position = transform.position;
            position.x = centerX;
            transform.position = position;
            _velocityX = 0f;
        }

        private void ApplyHorizontalMovement(float minX, float maxX)
        {
            if (_pitchBounds == null)
                return;

            var position = transform.position;
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, _pitchBounds.MinY, _pitchBounds.MaxY);

            var moveX = ReadMoveX();
            var isMoving = Mathf.Abs(moveX) > 0.001f;
            var desiredVelocity = isMoving ? moveX * speed : 0f;
            _velocityX = Mathf.MoveTowards(_velocityX, desiredVelocity, acceleration * Time.deltaTime);

            var previousX = position.x;
            position.x = Mathf.Clamp(position.x + _velocityX * Time.deltaTime, minX, maxX);
            position.y = Mathf.Clamp(position.y, _pitchBounds.MinY, _pitchBounds.MaxY);

            if (position.x <= minX && _velocityX < 0f || position.x >= maxX && _velocityX > 0f)
                _velocityX = 0f;
            else if (Mathf.Abs(position.x - previousX) < 0.0001f && Mathf.Abs(desiredVelocity) > 0.001f)
                _velocityX = 0f;

            transform.position = position;

            if (animator != null)
                animator.SetBool("Run", isMoving);
        }

        private float ReadMoveX() => _input?.MoveX ?? 0f;

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
            {
                KillMoveTween();
                _velocityX = 0f;
            }
        }

        private void OnDestroy()
        {
            KillMoveTween();

            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
