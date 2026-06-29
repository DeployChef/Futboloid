using UnityEngine;

namespace Futboloid.Gameplay.Ball
{
    /// <summary>
    /// Фиксированная стартовая позиция мяча (центр X, у ног вратаря).
    /// Стрелка — визуал и источник направления подачи.
    /// </summary>
    public class BallKickoffAnchor : MonoBehaviour, IBallAnchor
    {
        [SerializeField] private Transform directionArrow;
        [SerializeField] private Vector2 fallbackServeDirection = Vector2.up;

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
    }
}
