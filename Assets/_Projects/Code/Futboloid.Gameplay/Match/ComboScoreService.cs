using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    /// <summary>
    /// Аркадные очки и комбо-множитель внутри сессии мяча.
    /// Множитель растёт от ударов, со временем падает, касание вратаря снимает до −3 (мин. x1).
    /// </summary>
    public sealed class ComboScoreService : IDisposable
    {
        private readonly IGameEventBus _bus;
        private readonly ComboScoreSettings _settings;

        private readonly IDisposable _hitSubscription;
        private readonly IDisposable _goalSubscription;
        private readonly IDisposable _keeperSubscription;
        private readonly IDisposable _resetSubscription;
        private readonly IDisposable _phaseSubscription;
        private readonly IDisposable _matchEndedSubscription;

        private CancellationTokenSource _decayCts;
        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _matchEnded;

        public int TotalScore { get; private set; }
        public int Multiplier { get; private set; } = 1;

        public ComboScoreService(IGameEventBus bus, GameplaySettings gameplaySettings)
        {
            _bus = bus;
            _settings = gameplaySettings.ComboScore;
            Multiplier = _settings.MinMultiplier;

            _hitSubscription = bus.Subscribe<DefenderHitEvent>(OnDefenderHit);
            _goalSubscription = bus.Subscribe<GoalScoredEvent>(OnGoalScored);
            _keeperSubscription = bus.Subscribe<BallReturnedToKeeperEvent>(OnBallReturnedToKeeper);
            _resetSubscription = bus.Subscribe<PitchResetRequestedEvent>(_ => Reset());
            _phaseSubscription = bus.Subscribe<PitchPhaseChangedEvent>(e => _phase = e.Phase);
            _matchEndedSubscription = bus.Subscribe<MatchEndedEvent>(_ => _matchEnded = true);

            _decayCts = new CancellationTokenSource();
            RunDecayLoopAsync(_decayCts.Token).Forget();
        }

        public void Reset()
        {
            TotalScore = 0;
            _matchEnded = false;
            Multiplier = _settings.MinMultiplier;
            PublishChanged(0, Multiplier);
        }

        public void Dispose()
        {
            _hitSubscription?.Dispose();
            _goalSubscription?.Dispose();
            _keeperSubscription?.Dispose();
            _resetSubscription?.Dispose();
            _phaseSubscription?.Dispose();
            _matchEndedSubscription?.Dispose();

            _decayCts?.Cancel();
            _decayCts?.Dispose();
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
            if (!e.IsPlayerGoal)
                return;

            var bonus = _settings.GoalBonusPoints;
            if (bonus > 0)
                AddPoints(bonus * Multiplier);
        }

        private void OnBallReturnedToKeeper(BallReturnedToKeeperEvent _) =>
            DecreaseMultiplier(_settings.KeeperTouchPenalty);

        private async UniTaskVoid RunDecayLoopAsync(CancellationToken token)
        {
            var interval = _settings.DecayIntervalSeconds;

            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);

                if (_matchEnded || _phase != PitchPhase.Simulating)
                    continue;

                if (Multiplier <= _settings.MinMultiplier)
                    continue;

                DecreaseMultiplier(_settings.DecayStep);
            }
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

        private void DecreaseMultiplier(int amount)
        {
            if (amount <= 0 || Multiplier <= _settings.MinMultiplier)
                return;

            var previous = Multiplier;
            Multiplier = Mathf.Max(_settings.MinMultiplier, Multiplier - amount);

            if (Multiplier != previous)
                PublishChanged(0, previous);
        }

        private void PublishChanged(int deltaPoints, int previousMultiplier) =>
            _bus.Publish(new ComboScoreChangedEvent(TotalScore, Multiplier, deltaPoints, previousMultiplier));
    }
}
