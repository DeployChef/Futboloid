using System.Collections.Generic;

namespace Futboloid.Core.StatusEffects
{
    public interface IStatusEffectService
    {
        void Apply(StatusEffectDefinition definition, int stacks = 1);
        void Tick(float deltaTime);
        float GetMultiplier(StatId stat);
        float GetAdditive(StatId stat);
        IReadOnlyList<ActiveEffectSnapshot> GetActiveForHud();
        void ClearAll(StatusEffectRemoveReason reason);
    }
}
