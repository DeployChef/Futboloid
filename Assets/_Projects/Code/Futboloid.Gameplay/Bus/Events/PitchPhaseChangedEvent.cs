using Futboloid.Gameplay.Match;

namespace Futboloid.Gameplay.Bus.Events
{
    public readonly struct PitchPhaseChangedEvent
    {
        public PitchPhase Phase { get; }

        public PitchPhaseChangedEvent(PitchPhase phase)
        {
            Phase = phase;
        }
    }
}
