namespace Futboloid.UI.Views.StatusEffects
{
    public readonly struct StatusEffectHudEntry
    {
        public int InstanceId { get; }
        public string Title { get; }
        public string Description { get; }
        public bool IsDebuff { get; }

        public StatusEffectHudEntry(int instanceId, string title, string description, bool isDebuff)
        {
            InstanceId = instanceId;
            Title = title;
            Description = description;
            IsDebuff = isDebuff;
        }
    }
}
