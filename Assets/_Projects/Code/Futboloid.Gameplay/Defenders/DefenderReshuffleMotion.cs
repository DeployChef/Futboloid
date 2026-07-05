using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>
    /// DOTween-перемещение защитника при reshuffle после гола.
    /// </summary>
    public sealed class DefenderReshuffleMotion
    {
        private Tween _moveTween;

        public bool IsActive { get; private set; }

        /// <returns>false если отменено до прибытия в цель.</returns>
        public async UniTask<bool> PlayToAsync(
            Transform transform,
            Vector2 target,
            float arriveThreshold,
            float baseMoveDuration,
            CancellationToken ct)
        {
            Kill(transform);

            var current = (Vector2)transform.position;
            var offset = target - current;

            if (offset.sqrMagnitude <= arriveThreshold * arriveThreshold)
            {
                SnapTo(transform, target);
                return true;
            }

            IsActive = true;

            try
            {
                const float refDistance = 4f;
                var distance = offset.magnitude;
                var duration = baseMoveDuration * (distance / refDistance);
                duration = Mathf.Clamp(duration, 0.12f, 1.4f);

                var end = new Vector3(target.x, target.y, transform.position.z);
                _moveTween = transform
                    .DOMove(end, duration)
                    .SetEase(Ease.InOutQuad)
                    .SetLink(transform.gameObject);

                await TweenAsync.Await(_moveTween, ct);

                if (ct.IsCancellationRequested)
                    return false;

                SnapTo(transform, target);
                return true;
            }
            finally
            {
                IsActive = false;
                _moveTween = null;
            }
        }

        public void Kill(Transform transform)
        {
            if (_moveTween != null && _moveTween.IsActive())
                _moveTween.Kill();

            _moveTween = null;

            if (transform != null)
                transform.DOKill();

            IsActive = false;
        }

        private static void SnapTo(Transform transform, Vector2 target)
        {
            transform.position = new Vector3(target.x, target.y, transform.position.z);
        }
    }
}
