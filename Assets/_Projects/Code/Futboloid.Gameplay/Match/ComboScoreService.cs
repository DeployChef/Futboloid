using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Run;
using Futboloid.Core.StatusEffects;
using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    /// <summary>
    /// Аркадные очки (на весь забег) и комбо-множитель (на матч).
    /// </summary>
    public sealed class ComboScoreService : IDisposable
    {
        private readonly IGameEventBus _bus;
        private readonly ComboScoreSettings _settings;
        private readonly IRunProgressionService _progression;
        private readonly IStatusEffectService _statusEffects;
        private readonly ITournamentBracketReadModel _tournament;
        private readonly List<IDisposable> _subscriptions = new();

        private CancellationTokenSource _decayCts;
        private PitchPhase _phase = PitchPhase.KickoffWait;
        private bool _matchEnded;

        public int TotalScore { get; private set; }
        public int Multiplier { get; private set; } = 1;

        public ComboScoreService(
            IGameEventBus bus,
            GameplaySettings gameplaySettings,
            IRunProgressionService progression,
            IStatusEffectService statusEffects,
            ITournamentBracketReadModel tournament)
        {
            _bus = bus;
            _settings = gameplaySettings.ComboScore;
            _progression = progression;
            _statusEffects = statusEffects;
            _tournament = tournament;
            Multiplier = ResolveMinMultiplier();

            _subscriptions.Add(bus.Subscribe<DefenderHitEvent>(OnDefenderHit));
            _subscriptions.Add(bus.Subscribe<GoalScoredEvent>(OnGoalScored));
            _subscriptions.Add(bus.Subscribe<BallReturnedToKeeperEvent>(OnBallReturnedToKeeper));
            _subscriptions.Add(bus.Subscribe<PitchResetRequestedEvent>(OnPitchReset));
            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(e => _phase = e.Phase));
            _subscriptions.Add(bus.Subscribe<MatchEndedEvent>(_ => _matchEnded = true));
            _subscriptions.Add(bus.Subscribe<PerkPickedEvent>(OnPerkPicked));

            _decayCts = new CancellationTokenSource();
            RunDecayLoopAsync(_decayCts.Token).Forget();
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            _decayCts?.Cancel();
            _decayCts?.Dispose();
        }

        private void OnPitchReset(PitchResetRequestedEvent _)
        {
            if (_tournament.CurrentMatchNumber == 1)
                TotalScore = 0;

            _matchEnded = false;
            var previous = Multiplier;
            Multiplier = ResolveMinMultiplier();
            PublishChanged(0, previous);
        }

        private void OnPerkPicked(PerkPickedEvent e)
        {
            if (e.PerkId == PerkIds.ComboFloor)
                ApplyMinFloor();
        }

        private void OnDefenderHit(DefenderHitEvent e)
        {
            var pointValue = Mathf.Max(0, e.PointValue);
            if (pointValue <= 0)
                return;

            var comboGainMul = _statusEffects?.GetMultiplier(StatId.ComboGain) ?? 1f;
            AddPoints(Mathf.RoundToInt(pointValue * Multiplier * comboGainMul));
            IncreaseMultiplier();
        }

        private void OnGoalScored(GoalScoredEvent e)
        {
            if (!e.IsPlayerGoal)
                return;

            var bonus = _settings.GoalBonusPoints;
            if (bonus > 0)
            {
                var comboGainMul = _statusEffects?.GetMultiplier(StatId.ComboGain) ?? 1f;
                AddPoints(Mathf.RoundToInt(bonus * Multiplier * comboGainMul));
            }
        }

        private void OnBallReturnedToKeeper(BallReturnedToKeeperEvent _) =>
            DecreaseMultiplier(_settings.KeeperTouchPenalty);

        private async UniTaskVoid RunDecayLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var interval = ResolveDecayIntervalSeconds();
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);

                if (_matchEnded || _phase != PitchPhase.Simulating)
                    continue;

                if (Multiplier <= ResolveMinMultiplier())
                    continue;

                DecreaseMultiplier(_settings.DecayStep);
            }
        }

        private void ApplyMinFloor()
        {
            var min = ResolveMinMultiplier();
            if (Multiplier >= min)
                return;

            var previous = Multiplier;
            Multiplier = min;
            PublishChanged(0, previous);
        }

        private int ResolveMinMultiplier() =>
            Mathf.Max(_settings.MinMultiplier, _progression.GetComboMinMultiplier());

        private float ResolveDecayIntervalSeconds() =>
            _settings.DecayIntervalSeconds * _progression.GetComboDecayIntervalMultiplier();

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
            var min = ResolveMinMultiplier();
            if (amount <= 0 || Multiplier <= min)
                return;

            var previous = Multiplier;
            Multiplier = Mathf.Max(min, Multiplier - amount);

            if (Multiplier != previous)
                PublishChanged(0, previous);
        }

        private void PublishChanged(int deltaPoints, int previousMultiplier) =>
            _bus.Publish(new ComboScoreChangedEvent(TotalScore, Multiplier, deltaPoints, previousMultiplier));
    }
}
