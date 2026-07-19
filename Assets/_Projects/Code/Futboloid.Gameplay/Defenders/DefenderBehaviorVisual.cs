using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Futboloid.Gameplay.Defenders
{
    public sealed class DefenderBehaviorVisual : MonoBehaviour
    {
        [Serializable]
        private struct PaletteEntry
        {
            public DefenderBehaviorKind Kind;
            public Color Color;
        }

        private static readonly (DefenderBehaviorKind Kind, Color Color)[] DefaultColors =
        {
            (DefenderBehaviorKind.ReflectIdle, new Color(0.65f, 0.72f, 0.85f)),
            (DefenderBehaviorKind.ReflectWander, new Color(0.45f, 0.82f, 0.55f)),
            (DefenderBehaviorKind.ReflectChase, new Color(0.25f, 0.88f, 0.95f)),
            (DefenderBehaviorKind.ReflectPatrol, new Color(0.35f, 0.72f, 0.68f)),
            (DefenderBehaviorKind.ShootIdle, new Color(0.95f, 0.42f, 0.32f)),
            (DefenderBehaviorKind.ShootWander, new Color(0.98f, 0.72f, 0.28f)),
            (DefenderBehaviorKind.ShootChase, new Color(0.92f, 0.32f, 0.58f)),
            (DefenderBehaviorKind.ShootPatrol, new Color(0.72f, 0.42f, 0.92f))
        };

        [SerializeField] private Image typeImage;
        [SerializeField] private Color goalkeeperColor = new(0.95f, 0.85f, 0.2f, 1f);
        [SerializeField] private PaletteEntry[] palette = Array.Empty<PaletteEntry>();

        [Header("Hit Flash")]
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        public DefenderBehaviorKind CurrentKind { get; private set; }

        private Color _baseColor = Color.white;
        private bool _flashing;
        private CancellationTokenSource _flashCts;

        public void Apply(DefenderHitType hit, DefenderMovementType move, DefenderRole role)
        {
            if (typeImage == null)
                return;

            _baseColor = role == DefenderRole.Goalkeeper
                ? goalkeeperColor
                : ResolveColor(CurrentKind = DefenderBehaviorMapping.From(hit, move));

            if (!_flashing)
                typeImage.color = _baseColor;
        }

        /// <summary>
        /// Briefly flashes the visual with <see cref="flashColor"/>, lerping back to the base color
        /// over <see cref="flashDuration"/>. Re-triggering restarts the flash.
        /// </summary>
        public void FlashHit()
        {
            if (typeImage == null || flashDuration <= 0f)
                return;

            _flashCts?.Cancel();
            _flashCts?.Dispose();
            _flashCts = new CancellationTokenSource();
            FlashHitAsync(_flashCts.Token).Forget();
        }

        private async UniTaskVoid FlashHitAsync(CancellationToken ct)
        {
            _flashing = true;
            typeImage.color = flashColor;

            var elapsed = 0f;
            while (elapsed < flashDuration)
            {
                await UniTask.Yield(ct);
                if (ct.IsCancellationRequested)
                    break;

                elapsed += Time.deltaTime;
                typeImage.color = Color.Lerp(flashColor, _baseColor, elapsed / flashDuration);
            }

            if (!ct.IsCancellationRequested)
                typeImage.color = _baseColor;

            _flashing = false;
        }

        private void OnDestroy()
        {
            _flashCts?.Cancel();
            _flashCts?.Dispose();
            _flashCts = null;
        }

        private Color ResolveColor(DefenderBehaviorKind kind)
        {
            for (var i = 0; i < palette.Length; i++)
            {
                if (palette[i].Kind == kind)
                    return palette[i].Color;
            }

            for (var i = 0; i < DefaultColors.Length; i++)
            {
                if (DefaultColors[i].Kind == kind)
                    return DefaultColors[i].Color;
            }

            return Color.white;
        }

        private void Reset()
        {
            if (typeImage == null)
                typeImage = GetComponent<Image>();

            palette = CreateDefaultPalette();
        }

        private void OnValidate()
        {
            if (typeImage == null)
                typeImage = GetComponent<Image>();

            if (palette == null || palette.Length == 0)
                palette = CreateDefaultPalette();
        }

        private static PaletteEntry[] CreateDefaultPalette()
        {
            var entries = new PaletteEntry[DefaultColors.Length];
            for (var i = 0; i < DefaultColors.Length; i++)
            {
                entries[i] = new PaletteEntry
                {
                    Kind = DefaultColors[i].Kind,
                    Color = DefaultColors[i].Color
                };
            }

            return entries;
        }
    }
}
