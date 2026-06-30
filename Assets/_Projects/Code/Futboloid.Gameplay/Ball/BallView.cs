using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Defenders;
using Futboloid.Gameplay.Match;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Ball
{
    public class BallView : MonoBehaviour
    {
        [SerializeField] private BallSettings settings = new();
        [SerializeField] private BallKickoffAnchor kickoffAnchor;

        [Header("Reshuffle tween")]
        [SerializeField] private float reshuffleShrinkDuration = 0.15f;
        [SerializeField] private float reshuffleGrowDuration = 0.22f;
        [SerializeField] private float reshuffleMinScale = 0.05f;

        private readonly List<IDisposable> _subscriptions = new();

        private IGameEventBus _bus;
        private BallMotion _motion;
        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _onField;
        private bool _simulating;
        private bool _reshuffleAnimating;
        private Vector3 _defaultLocalScale = Vector3.one;
        private Tween _reshuffleTween;

        public Vector2 Position => _motion != null ? _motion.Position : (Vector2)transform.position;

        [Inject]
        public void Construct(
            IGameEventBus bus,
            DefenderGridRegistry defenderRegistry,
            PitchStateMachine pitch,
            MatchFlow matchFlow)
        {
            _bus = bus;
            _motion = new BallMotion(settings, bus, defenderRegistry);

            if (kickoffAnchor == null)
                kickoffAnchor = FindAnyObjectByType<BallKickoffAnchor>();

            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            _defaultLocalScale = transform.localScale;
            SyncPitchState(pitch.Current);
            _onField = matchFlow.IsOnField;
            ResetAtKickoff();
        }

        public void TryServe(Vector2 direction)
        {
            if (!_onField || _phase != PitchPhase.KickoffWait || kickoffAnchor == null)
                return;

            _motion.Serve(kickoffAnchor.WorldPosition, direction);
            ApplyTransform();
        }

        public async UniTask PlayReshuffleToKickoffAsync(CancellationToken ct)
        {
            CancelReshuffleTween();

            if (kickoffAnchor == null)
            {
                ResetAtKickoff();
                return;
            }

            _reshuffleAnimating = true;

            try
            {
                var target = kickoffAnchor.WorldPosition;
                var shrinkTween = transform
                    .DOScale(_defaultLocalScale * reshuffleMinScale, reshuffleShrinkDuration)
                    .SetEase(Ease.InQuad)
                    .SetLink(gameObject);
                await TweenAsync.Await(shrinkTween, ct);

                _motion.ResetAt(target);
                transform.position = new Vector3(target.x, target.y, transform.position.z);

                var growTween = transform
                    .DOScale(_defaultLocalScale, reshuffleGrowDuration)
                    .SetEase(Ease.OutBack)
                    .SetLink(gameObject);
                _reshuffleTween = growTween;
                await TweenAsync.Await(growTween, ct);
            }
            finally
            {
                _reshuffleAnimating = false;
                _reshuffleTween = null;
                transform.localScale = _defaultLocalScale;
                ApplyTransform();
            }
        }

        public void CancelReshuffleTween()
        {
            if (_reshuffleTween != null && _reshuffleTween.IsActive())
                _reshuffleTween.Kill();

            _reshuffleTween = null;
            transform.DOKill();
            _reshuffleAnimating = false;
            transform.localScale = _defaultLocalScale;
        }

        private void Update()
        {
            if (!_onField || _motion == null || _reshuffleAnimating)
                return;

            if (_motion.IsHeld)
            {
                _motion.Tick(Time.deltaTime);
                ApplyTransform();
                return;
            }

            if (!_simulating)
                return;

            _motion.Tick(Time.deltaTime);
            ApplyTransform();
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            SyncPitchState(e.Phase);
        }

        private void SyncPitchState(PitchPhase phase)
        {
            _phase = phase;
            _simulating = phase == PitchPhase.Simulating;

            if (phase == PitchPhase.KickoffWait && !_reshuffleAnimating)
                ResetAtKickoff();
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            _onField = e.Current == NavigationState.OnField;

            if (_onField && _phase == PitchPhase.KickoffWait && !_reshuffleAnimating)
                ResetAtKickoff();
        }

        private void ResetAtKickoff()
        {
            if (_motion == null || kickoffAnchor == null)
                return;

            _motion.ResetAt(kickoffAnchor.WorldPosition);
            ApplyTransform();
        }

        private void ApplyTransform()
        {
            var position = _motion.Position;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        private void OnDestroy()
        {
            CancelReshuffleTween();

            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.65f);
            Gizmos.DrawWireSphere(transform.position, settings.Radius);
        }
#endif
    }
}
