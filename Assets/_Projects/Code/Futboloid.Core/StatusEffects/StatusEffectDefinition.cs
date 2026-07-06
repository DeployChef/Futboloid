using UnityEngine;

namespace Futboloid.Core.StatusEffects
{
    [CreateAssetMenu(fileName = "StatusEffect", menuName = "Futboloid/Status Effect Definition")]
    public class StatusEffectDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [TextArea(2, 4)]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isDebuff;
        [SerializeField] private float durationSeconds = 8f;
        [SerializeField] private StatId affectedStat = StatId.GoalkeeperMoveSpeed;
        [SerializeField] private float multiplier = 1f;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public bool IsDebuff => isDebuff;
        public float DurationSeconds => Mathf.Max(0f, durationSeconds);
        public StatId AffectedStat => affectedStat;
        public float Multiplier => multiplier > 0f ? multiplier : 1f;
    }
}
