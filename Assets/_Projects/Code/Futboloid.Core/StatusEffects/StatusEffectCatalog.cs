using System.Collections.Generic;
using UnityEngine;

namespace Futboloid.Core.StatusEffects
{
    [CreateAssetMenu(fileName = "StatusEffectCatalog", menuName = "Futboloid/Status Effect Catalog")]
    public class StatusEffectCatalog : ScriptableObject
    {
        public const string ResourcePath = "Data/Settings/StatusEffectCatalog";

        [SerializeField] private List<StatusEffectDefinition> effects = new();

        public IReadOnlyList<StatusEffectDefinition> Effects => effects;

        public StatusEffectDefinition FindById(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return null;

            foreach (var effect in effects)
            {
                if (effect != null && effect.Id == effectId)
                    return effect;
            }

            return null;
        }

        public static StatusEffectCatalog Load()
        {
            var catalog = Resources.Load<StatusEffectCatalog>(ResourcePath);
            if (catalog != null)
                return catalog;

            Debug.LogWarning(
                $"[StatusEffectCatalog] Asset not found at Resources/{ResourcePath}. " +
                "Create via Assets → Create → Futboloid → Status Effect Catalog.");
            return CreateInstance<StatusEffectCatalog>();
        }
    }
}
