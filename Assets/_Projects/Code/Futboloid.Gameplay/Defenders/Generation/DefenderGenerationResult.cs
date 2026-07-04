using System.Collections.Generic;

namespace Futboloid.Gameplay.Defenders
{
    public sealed class DefenderGenerationResult
    {
        public DefenderBuild Goalkeeper;
        public readonly List<DefenderBuild> Field = new();
    }
}
