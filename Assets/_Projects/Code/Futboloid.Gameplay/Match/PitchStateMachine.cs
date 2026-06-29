using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Bus.Events;
using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    public class PitchStateMachine
    {
        private readonly IGameEventBus _bus;
        private readonly MatchFlow _matchFlow;

        public PitchPhase Current { get; private set; } = PitchPhase.KickoffWait;

        public PitchStateMachine(IGameEventBus bus, MatchFlow matchFlow)
        {
            _bus = bus;
            _matchFlow = matchFlow;
        }

        public bool IsSimulating => Current == PitchPhase.Simulating;

        public void Reset()
        {
            _matchFlow.Reset();
            TransitionTo(PitchPhase.KickoffWait);
        }

        public void EnterKickoffWait() => TransitionTo(PitchPhase.KickoffWait);

        public void StartSimulation() => TransitionTo(PitchPhase.Simulating);

        public void EnterReshuffle() => TransitionTo(PitchPhase.Reshuffle);

        public void EnterBonusPick() => TransitionTo(PitchPhase.BonusPick);

        public void EnterMatchEnded() => TransitionTo(PitchPhase.MatchEnded);

        public void CompleteReshuffle() => EnterKickoffWait();

        private void TransitionTo(PitchPhase phase)
        {
            if (Current == phase)
                return;

            Current = phase;
            _bus.Publish(new PitchPhaseChangedEvent(phase));
            Debug.Log($"[PitchStateMachine] → {phase}");
        }
    }
}
