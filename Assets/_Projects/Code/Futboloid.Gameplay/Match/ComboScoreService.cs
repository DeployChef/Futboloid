using System;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    /// <summary>
    /// Аркадные очки и комбо-множитель внутри сессии мяча.
    /// Слушает шину — не вшит в BallMotion / DefenderView.
    /// </summary>
    public sealed class ComboScoreService : IDisposable
    {
        private readonly IGameEventBus _bus;
        private readonly ComboScoreSettings _settings;

        private readonly IDisposable _hitSubscription;
        private readonly IDisposable _goalSubscription;
        private readonly IDisposable _keeperSubscription;
        private readonly IDisposable _resetSubscription;

        public int TotalScore { get; private set; }
        public int Multiplier { get; private set; } = 1;

        public ComboScoreService(IGameEventBus bus, GameplaySettings gameplaySettings)
        {
            _bus = bus;
            _settings = gameplaySettings.ComboScore;

            _hitSubscription = bus.Subscribe<DefenderHitEvent>(OnDefenderHit);
            _goalSubscription = bus.Subscribe<GoalScoredEvent>(OnGoalScored);
            _keeperSubscription = bus.Subscribe<BallReturnedToKeeperEvent>(OnBallReturnedToKeeper);
            _resetSubscription = bus.Subscribe<PitchResetRequestedEvent>(_ => Reset());
        }

        public void Reset()
        {
            TotalScore = 0;
            Multiplier = 1;
            PublishChanged(0, Multiplier);
        }

        public void Dispose()
        {
            _hitSubscription?.Dispose();
            _goalSubscription?.Dispose();
            _keeperSubscription?.Dispose();
            _resetSubscription?.Dispose();
        }

        private void OnDefenderHit(DefenderHitEvent e)
        {
            var pointValue = Mathf.Max(0, e.PointValue);
            if (pointValue <= 0)
                return;

            AddPoints(pointValue * Multiplier);
            IncreaseMultiplier();
        }

        private void OnGoalScored(GoalScoredEvent e)
        {
            if (e.IsPlayerGoal)
            {
                var bonus = _settings.GoalBonusPoints;
                if (bonus > 0)
                    AddPoints(bonus * Multiplier);
            }

            ResetMultiplier();
        }

        private void OnBallReturnedToKeeper(BallReturnedToKeeperEvent _) =>
            ResetMultiplier();

        private void ResetMultiplier()
        {
            if (Multiplier == 1)
                return;

            var previous = Multiplier;
            Multiplier = 1;
            PublishChanged(0, previous);
        }

        private void AddPoints(int delta)
        {
            if (delta <= 0)
                return;

            TotalScore += delta;
            PublishChanged(delta, Multiplier);
        }

        private void IncreaseMultiplier()
        {
            var previous = Multiplier;
            var next = Multiplier + 1;
            var cap = _settings.MaxMultiplier;
            if (cap > 0)
                next = Mathf.Min(next, cap);

            if (next == Multiplier)
                return;

            Multiplier = next;
            PublishChanged(0, previous);
        }

        private void PublishChanged(int deltaPoints, int previousMultiplier) =>
            _bus.Publish(new ComboScoreChangedEvent(TotalScore, Multiplier, deltaPoints, previousMultiplier));
    }
}
