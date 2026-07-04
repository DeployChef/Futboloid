using System.Collections.Generic;
using UnityEngine;

namespace Futboloid.Core.Run
{
    [CreateAssetMenu(fileName = "PerkCatalog", menuName = "Futboloid/Perk Catalog")]
    public class PerkCatalog : ScriptableObject
    {
        public const string ResourcePath = "Data/Settings/PerkCatalog";

        [SerializeField] private List<PerkDefinition> perks = new();

        public IReadOnlyList<PerkDefinition> Perks => perks;

        public static PerkCatalog Load()
        {
            var catalog = Resources.Load<PerkCatalog>(ResourcePath);
            if (catalog != null)
                return catalog;

            Debug.LogWarning(
                $"[PerkCatalog] Asset not found at Resources/{ResourcePath}. " +
                "Create via Assets → Create → Futboloid → Perk Catalog.");
            return CreateInstance<PerkCatalog>();
        }
    }
}
