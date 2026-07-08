using System;
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

        public DefenderBehaviorKind CurrentKind { get; private set; }

        public void Apply(DefenderHitType hit, DefenderMovementType move, DefenderRole role)
        {
            if (typeImage == null)
                return;

            typeImage.color = role == DefenderRole.Goalkeeper
                ? goalkeeperColor
                : ResolveColor(CurrentKind = DefenderBehaviorMapping.From(hit, move));
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
