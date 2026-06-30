using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Ball
{
    public class BallView : MonoBehaviour
    {
        [SerializeField] private BallSettings settings = new();
        [SerializeField] private BallKickoffAnchor kickoffAnchor;

        private readonly List<IDisposable> _subscriptions = new();

        private IGameEventBus _bus;
        private BallMotion _motion;
        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _onField;
        private bool _simulating;

        public Vector2 Position => _motion != null ? _motion.Position : (Vector2)transform.position;

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            _bus = bus;
            _motion = new BallMotion(settings, bus);

            if (kickoffAnchor == null)
                kickoffAnchor = FindAnyObjectByType<BallKickoffAnchor>();

            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            ResetAtKickoff();
        }

        public void TryServe(Vector2 direction)
        {
            if (!_onField || _phase != PitchPhase.KickoffWait || kickoffAnchor == null)
                return;

            _motion.Serve(kickoffAnchor.WorldPosition, direction);
            ApplyTransform();
        }

        private void Update()
        {
            if (!_onField || _motion == null)
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
            _phase = e.Phase;
            _simulating = e.Phase == PitchPhase.Simulating;

            if (e.Phase == PitchPhase.KickoffWait)
                ResetAtKickoff();
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            _onField = e.Current == NavigationState.OnField;

            if (_onField && _phase == PitchPhase.KickoffWait)
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
