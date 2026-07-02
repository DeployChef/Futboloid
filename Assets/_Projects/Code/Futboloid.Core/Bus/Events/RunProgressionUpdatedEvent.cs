using UnityEngine;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct RunPerkHudEntry
    {
        public string PerkId { get; }
        public int Level { get; }
        public Sprite CardFrame { get; }
        public string Title { get; }
        public string Description { get; }

        public RunPerkHudEntry(
            string perkId,
            int level,
            Sprite cardFrame,
            string title,
            string description)
        {
            PerkId = perkId;
            Level = level;
            CardFrame = cardFrame;
            Title = title;
            Description = description;
        }
    }

    public readonly struct RunProgressionUpdatedEvent
    {
        public int RunLevel { get; }
        public int CurrentXp { get; }
        public int XpToNextLevel { get; }
        public float Fill01 { get; }
        public RunPerkHudEntry[] Perks { get; }

        public RunProgressionUpdatedEvent(
            int runLevel,
            int currentXp,
            int xpToNextLevel,
            float fill01,
            RunPerkHudEntry[] perks)
        {
            RunLevel = runLevel;
            CurrentXp = currentXp;
            XpToNextLevel = xpToNextLevel;
            Fill01 = fill01;
            Perks = perks ?? System.Array.Empty<RunPerkHudEntry>();
        }
    }
}
