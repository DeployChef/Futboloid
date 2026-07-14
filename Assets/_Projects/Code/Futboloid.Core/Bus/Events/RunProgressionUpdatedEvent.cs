using UnityEngine;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct RunPerkHudEntry
    {
        public string PerkId { get; }
        public int Level { get; }
        public Sprite Icon { get; }

        public RunPerkHudEntry(string perkId, int level, Sprite icon)
        {
            PerkId = perkId;
            Level = level;
            Icon = icon;
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
