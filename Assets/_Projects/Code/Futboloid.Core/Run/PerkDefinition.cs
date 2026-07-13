using Futboloid.Core.Localization;
using UnityEngine;

namespace Futboloid.Core.Run
{
    [CreateAssetMenu(fileName = "Perk", menuName = "Futboloid/Perk Definition")]
    public class PerkDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite cardFrame;
        [SerializeField] private Sprite icon;
        [SerializeField] private int maxLevel = 5;
        [SerializeField] private float valuePerLevel = 1f;

        public string Id => id;
        public string NameLocalizationKey => LocalizationKeys.PerkName(id);
        public string DescriptionLocalizationKey => LocalizationKeys.PerkDescription(id);
        public Sprite CardFrame => cardFrame;
        public Sprite Icon => icon;
        public int MaxLevel => Mathf.Max(1, maxLevel);
        public float ValuePerLevel => valuePerLevel;
    }
}
