using System;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Run;
using UnityEngine;

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

        private readonly IDisposable _offeredSubscription;
        private readonly IDisposable _pickedSubscription;

        private bool _pausedForBonusPick;

        public BonusPickCoordinator(
            IGameEventBus bus,
            PitchStateMachine pitch,
            IRunProgressionService run,
            MatchFlow matchFlow)
        {
            _bus = bus;
            _pitch = pitch;
            _run = run;
            _matchFlow = matchFlow;

            _offeredSubscription = bus.Subscribe<BonusPickOfferedEvent>(OnBonusPickOffered);
            _pickedSubscription = bus.Subscribe<PerkPickedEvent>(OnPerkPicked);
        }

        public void Dispose()
        {
            _offeredSubscription?.Dispose();
            _pickedSubscription?.Dispose();

            if (_pausedForBonusPick)
                ResumeTimeScale();
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

            ResumeTimeScale();

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
            Time.timeScale = 0f;
        }

        private void ResumeTimeScale()
        {
            if (!_pausedForBonusPick)
                return;

            _pausedForBonusPick = false;
            Time.timeScale = 1f;
        }
    }
}
