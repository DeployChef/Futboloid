using System.Collections.Generic;

namespace Futboloid.Core.StatusEffects
{
    /// <summary>
    /// Запоминает timed-эффекты, чья карточка уже показывалась игроку (на всю сессию приложения).
    /// </summary>
    public interface IStatusEffectRevealMemory
    {
        bool WasRevealed(string effectId);
        void MarkRevealed(string effectId);
    }

    public sealed class StatusEffectRevealMemory : IStatusEffectRevealMemory
    {
        private readonly HashSet<string> _revealedEffectIds = new();

        public bool WasRevealed(string effectId) =>
            !string.IsNullOrEmpty(effectId) && _revealedEffectIds.Contains(effectId);

        public void MarkRevealed(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return;

            _revealedEffectIds.Add(effectId);
        }
    }
}
