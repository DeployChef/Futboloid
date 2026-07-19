using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>
    /// Flashes the body <see cref="SpriteRenderer"/> with a hit color, lerping back to the
    /// base color over <see cref="flashDuration"/>. Re-triggering restarts the flash.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DefenderHitFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        private SpriteRenderer _sprite;
        private Color _baseColor;
        private bool _flashing;
        private bool _baseCaptured;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
        }

        public void FlashHit()
        {
            if (_sprite == null || flashDuration <= 0f)
                return;

            if (!_baseCaptured)
            {
                _baseColor = _sprite.color;
                _baseCaptured = true;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            FlashHitAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid FlashHitAsync(CancellationToken ct)
        {
            _flashing = true;
            _sprite.color = flashColor;

            var elapsed = 0f;
            while (elapsed < flashDuration)
            {
                await UniTask.Yield(ct);
                if (ct.IsCancellationRequested)
                    break;

                elapsed += Time.deltaTime;
                _sprite.color = Color.Lerp(flashColor, _baseColor, elapsed / flashDuration);
            }

            if (!ct.IsCancellationRequested)
                _sprite.color = _baseColor;

            _flashing = false;
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
