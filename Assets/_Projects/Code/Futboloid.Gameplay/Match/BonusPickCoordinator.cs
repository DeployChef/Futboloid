using System;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Pause;
using Futboloid.Core.Run;

namespace Futboloid.Gameplay.Match
{
    /// <summary>
    /// Связывает RunStateService (App) с Pitch FSM: пауза на BonusPick, возобновление после выбора.
    /// </summary>
    public class BonusPickCoordinator : IDisposable
    {
        private readonly IGameEventBus _bus;
        private readonly PitchStateMachine _pitch;
        private readonly IRunProgressionService _run;
        private readonly MatchFlow _matchFlow;
        private readonly PauseCoordinator _pause;

        private readonly IDisposable _offeredSubscription;
        private readonly IDisposable _pickedSubscription;

        private bool _pausedForBonusPick;

        public BonusPickCoordinator(
            IGameEventBus bus,
            PitchStateMachine pitch,
            IRunProgressionService run,
            MatchFlow matchFlow,
            PauseCoordinator pause)
        {
            _bus = bus;
            _pitch = pitch;
            _run = run;
            _matchFlow = matchFlow;
            _pause = pause;

            _offeredSubscription = bus.Subscribe<BonusPickOfferedEvent>(OnBonusPickOffered);
            _pickedSubscription = bus.Subscribe<PerkPickedEvent>(OnPerkPicked);
        }

        public void Dispose()
        {
            _offeredSubscription?.Dispose();
            _pickedSubscription?.Dispose();

            if (_pausedForBonusPick)
                ReleaseBonusPickPause();
        }

        private void OnBonusPickOffered(BonusPickOfferedEvent e)
        {
            if (e.Count == 0)
                return;

            if (_pitch.Current == PitchPhase.Simulating || _pitch.Current == PitchPhase.BonusPick)
            {
                PauseForBonusPick();
                if (_pitch.Current != PitchPhase.BonusPick)
                    _pitch.EnterBonusPick();
            }
        }

        private void OnPerkPicked(PerkPickedEvent e)
        {
            if (_run.PendingPerkPicks > 0 || _run.IsBonusPickActive)
                return;

            ReleaseBonusPickPause();

            if (_matchFlow.TryCompleteWipeVictory(_run))
                return;

            if (_pitch.Current == PitchPhase.BonusPick)
                _pitch.StartSimulation();
        }

        private void PauseForBonusPick()
        {
            if (_pausedForBonusPick)
                return;

            _pausedForBonusPick = true;
            _pause.Request(PauseReasons.BonusPick);
        }

        private void ReleaseBonusPickPause()
        {
            if (!_pausedForBonusPick)
                return;

            _pausedForBonusPick = false;
            _pause.Release(PauseReasons.BonusPick);
        }
    }
}
