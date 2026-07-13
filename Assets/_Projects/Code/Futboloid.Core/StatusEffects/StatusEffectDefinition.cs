using Futboloid.Core.Localization;
using UnityEngine;

namespace Futboloid.Core.StatusEffects
{
    [CreateAssetMenu(fileName = "StatusEffect", menuName = "Futboloid/Status Effect Definition")]
    public class StatusEffectDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isDebuff;
        [SerializeField] private float durationSeconds = 8f;
        [SerializeField] private StatId affectedStat = StatId.GoalkeeperMoveSpeed;
        [SerializeField] private float multiplier = 1f;

        public string Id => id;
        public string NameLocalizationKey => LocalizationKeys.StatusEffectName(id);
        public string DescriptionLocalizationKey => LocalizationKeys.StatusEffectDescription(id);
        public Sprite Icon => icon;
        public bool IsDebuff => isDebuff;
        public float DurationSeconds => Mathf.Max(0f, durationSeconds);
        public StatId AffectedStat => affectedStat;
        public float Multiplier => multiplier > 0f ? multiplier : 1f;
    }
}
