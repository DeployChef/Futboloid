namespace Futboloid.UI.Views.StatusEffects
{
    public readonly struct StatusEffectHudEntry
    {
        public int InstanceId { get; }
        public string EffectId { get; }
        public bool IsDebuff { get; }

        public StatusEffectHudEntry(int instanceId, string effectId, bool isDebuff)
        {
            InstanceId = instanceId;
            EffectId = effectId;
            IsDebuff = isDebuff;
        }
    }
}
