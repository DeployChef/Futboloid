using System;
using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;

namespace Futboloid.Core.Analytics
{
    /// <summary>
    /// Слушает шину и шлёт каталог v1 в <see cref="IAnalyticsService"/>.
    /// Wiki: «Аналитика».
    /// </summary>
    public sealed class AnalyticsEventBridge : IDisposable
    {
        private readonly IAnalyticsService _analytics;
        private readonly ITournamentBracketReadModel _tournament;
        private readonly List<IDisposable> _subscriptions = new();

        private readonly float _sessionStartedAt;
        private int _maxCombo = 1;
        private int _bonusPicksThisMatch;

        public AnalyticsEventBridge(
            IGameEventBus bus,
            IAnalyticsService analytics,
            ITournamentBracketReadModel tournament)
        {
            _analytics = analytics;
            _tournament = tournament;
            _sessionStartedAt = Time.realtimeSinceStartup;

            TrackSessionStart();

            _subscriptions.Add(bus.Subscribe<TournamentRunStartedEvent>(OnTournamentStarted));
            _subscriptions.Add(bus.Subscribe<MatchStartedEvent>(_ => ResetMatchCounters()));
            _subscriptions.Add(bus.Subscribe<PitchResetRequestedEvent>(_ => ResetMatchCounters()));
            _subscriptions.Add(bus.Subscribe<ComboScoreChangedEvent>(OnComboChanged));
            _subscriptions.Add(bus.Subscribe<MatchEndedEvent>(OnMatchEnded));
            _subscriptions.Add(bus.Subscribe<BonusPickOfferedEvent>(OnPerkOffered));
            _subscriptions.Add(bus.Subscribe<PerkPickedEvent>(OnPerkPicked));
            _subscriptions.Add(bus.Subscribe<StatusEffectAppliedEvent>(OnStatusEffectApplied));
        }

        public void Dispose()
        {
            TrackSessionEnd();

            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();
            _analytics.Flush();
        }

        private void TrackSessionStart()
        {
            _analytics.Track(new AnalyticsEvent(
                AnalyticsEventNames.SessionStart,
                new Dictionary<string, object>
                {
                    ["platform"] = Application.platform.ToString(),
                    ["build_version"] = Application.version
                }));
        }

        private void TrackSessionEnd()
        {
            var duration = Mathf.Max(0f, Time.realtimeSinceStartup - _sessionStartedAt);
            _analytics.Track(new AnalyticsEvent(
                AnalyticsEventNames.SessionEnd,
                new Dictionary<string, object>
                {
                    ["duration_sec"] = Mathf.RoundToInt(duration)
                }));
        }

        private void OnTournamentStarted(TournamentRunStartedEvent e)
        {
            _analytics.Track(new AnalyticsEvent(
                AnalyticsEventNames.TournamentStart,
                new Dictionary<string, object>
                {
                    ["matches_to_win"] = e.MatchesToWin,
                    ["run_seed"] = e.RunSeed,
                    ["start_match"] = e.StartMatchNumber
                }));
        }

        private void ResetMatchCounters()
        {
            _maxCombo = 1;
            _bonusPicksThisMatch = 0;
        }

        private void OnComboChanged(ComboScoreChangedEvent e)
        {
            if (e.Multiplier > _maxCombo)
                _maxCombo = e.Multiplier;
        }

        private void OnMatchEnded(MatchEndedEvent e)
        {
            var round = _tournament.CurrentMatchNumber;
            var matchesToWin = _tournament.MatchesToWin;

            _analytics.Track(new AnalyticsEvent(
                AnalyticsEventNames.MatchEnd,
                new Dictionary<string, object>
                {
                    ["round"] = round,
                    ["player_score"] = e.PlayerScore,
                    ["enemy_score"] = e.OpponentScore,
                    ["win"] = e.PlayerWon,
                    ["end_reason"] = e.Reason == MatchEndReason.Wipe ? "wipe" : "timer",
                    ["duration_sec"] = Mathf.RoundToInt(e.DurationSeconds),
                    ["max_combo"] = _maxCombo,
                    ["bonus_picks"] = _bonusPicksThisMatch
                }));

            TryTrackTournamentEnd(e.PlayerWon, round, matchesToWin);
        }

        private void TryTrackTournamentEnd(bool playerWon, int round, int matchesToWin)
        {
            string result = null;
            if (!playerWon)
                result = "eliminated";
            else if (round >= matchesToWin)
                result = "completed";

            if (result == null)
                return;

            _analytics.Track(new AnalyticsEvent(
                AnalyticsEventNames.TournamentEnd,
                new Dictionary<string, object>
                {
                    ["result"] = result,
                    ["matches_played"] = round,
                    ["max_round"] = round
                }));
        }

        private void OnPerkOffered(BonusPickOfferedEvent e)
        {
            _analytics.Track(new AnalyticsEvent(
                AnalyticsEventNames.PerkOffered,
                new Dictionary<string, object>
                {
                    ["offer_0"] = e.Offer0 != null ? e.Offer0.Id : string.Empty,
                    ["offer_1"] = e.Offer1 != null ? e.Offer1.Id : string.Empty,
                    ["offer_2"] = e.Offer2 != null ? e.Offer2.Id : string.Empty,
                    ["round"] = _tournament.CurrentMatchNumber
                }));
        }

        private void OnPerkPicked(PerkPickedEvent e)
        {
            _bonusPicksThisMatch++;
            _analytics.Track(new AnalyticsEvent(
                AnalyticsEventNames.PerkPicked,
                new Dictionary<string, object>
                {
                    ["perk_id"] = e.PerkId,
                    ["level_after"] = e.NewLevel,
                    ["round"] = _tournament.CurrentMatchNumber
                }));
        }

        private void OnStatusEffectApplied(StatusEffectAppliedEvent e)
        {
            _analytics.Track(new AnalyticsEvent(
                AnalyticsEventNames.StatusEffectApplied,
                new Dictionary<string, object>
                {
                    ["effect_id"] = e.EffectId,
                    ["is_debuff"] = e.IsDebuff,
                    ["round"] = _tournament.CurrentMatchNumber
                }));
        }
    }
}
