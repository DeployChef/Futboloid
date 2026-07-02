using UnityEngine;

namespace Futboloid.Core.Run
{
    [CreateAssetMenu(fileName = "Perk", menuName = "Futboloid/Perk Definition")]
    public class PerkDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [TextArea(2, 4)]
        [SerializeField] private string description;
        [SerializeField] private Sprite cardFrame;
        [SerializeField] private int maxLevel = 5;
        [SerializeField] private float valuePerLevel = 1f;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite CardFrame => cardFrame;
        public int MaxLevel => Mathf.Max(1, maxLevel);
        public float ValuePerLevel => valuePerLevel;

        public string GetLevelLabel(int levelAfterPick) =>
            $"{displayName} (ур. {levelAfterPick})";
    }
}
