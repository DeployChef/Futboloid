using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Futboloid.Gameplay.Keeper
{
    /// <summary>
    /// DOTween-перемещение вратаря в центр при reshuffle после гола.
    /// </summary>
    public sealed class GoalkeeperReshuffleMotion
    {
        private Tween _moveTween;

        public bool IsActive { get; private set; }

        public async UniTask PlayToCenterAsync(
            Transform transform,
            float centerX,
            float arriveThreshold,
            float baseMoveDuration,
            CancellationToken ct)
        {
            Kill(transform);

            var delta = centerX - transform.position.x;
            if (Mathf.Abs(delta) <= arriveThreshold)
            {
                SnapToCenterX(transform, centerX);
                return;
            }

            IsActive = true;

            try
            {
                const float refDistance = 4f;
                var distance = Mathf.Abs(delta);
                var duration = baseMoveDuration * (distance / refDistance);
                duration = Mathf.Clamp(duration, 0.12f, 1.4f);

                _moveTween = transform
                    .DOMoveX(centerX, duration)
                    .SetEase(Ease.InOutQuad)
                    .SetLink(transform.gameObject);

                await TweenAsync.Await(_moveTween, ct);

                if (!ct.IsCancellationRequested)
                    SnapToCenterX(transform, centerX);
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

        private static void SnapToCenterX(Transform transform, float centerX)
        {
            var position = transform.position;
            position.x = centerX;
            transform.position = position;
        }
    }
}
