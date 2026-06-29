using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Bus.Events;
using Futboloid.Gameplay.Match;
using Futboloid.Gameplay.Scene;
using UnityEngine;

namespace Futboloid.Gameplay.Ball
{
    /// <summary>
    /// Фиксированная стартовая позиция мяча (центр X, у ног вратаря).
    /// Стрелка — визуал и источник направления подачи.
    /// </summary>
    public class BallKickoffAnchor : MonoBehaviour, IBallAnchor, IGameSceneInitializable
    {
        [SerializeField] private Transform directionArrow;
        [SerializeField] private Vector2 fallbackServeDirection = Vector2.up;
        [SerializeField] private float maxAimAngleDegrees = 45f;

        private readonly List<IDisposable> _subscriptions = new();

        public Vector2 WorldPosition => transform.position;

        public Vector2 ServeDirection
        {
            get
            {
                if (directionArrow != null)
                    return ((Vector2)directionArrow.up).normalized;

                return fallbackServeDirection.sqrMagnitude > 0.0001f
                    ? fallbackServeDirection.normalized
                    : Vector2.up;
            }
        }

        /// <summary>
        /// Наклон стрелки от смещения вратаря относительно якоря (A/D в кик-оффе).
        /// </summary>
        public void UpdateAimFromKeeperX(float keeperWorldX, float horizontalOffsetRange)
        {
            if (directionArrow == null)
                return;

            var anchorX = transform.position.x;
            var delta = keeperWorldX - anchorX;
            var normalized = horizontalOffsetRange > 0.001f
                ? Mathf.Clamp(delta / horizontalOffsetRange, -1f, 1f)
                : 0f;
            var angle = normalized * maxAimAngleDegrees;
            directionArrow.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void Initialize(IGameEventBus bus)
        {
            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            SetDirectionArrowVisible(true);
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            SetDirectionArrowVisible(e.Phase == PitchPhase.KickoffWait);
        }

        private void SetDirectionArrowVisible(bool visible)
        {
            if (directionArrow != null)
                directionArrow.gameObject.SetActive(visible);
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
