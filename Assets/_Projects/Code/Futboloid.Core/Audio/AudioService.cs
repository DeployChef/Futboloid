using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;

namespace Futboloid.Core.Audio
{
    /// <summary>
    /// Слушает шину и воспроизводит звуки из <see cref="AudioCatalog"/>.
    /// Полный маппинг событий — в Wiki: «Каталог событий и звуков».
    /// </summary>
    public sealed class AudioService : IDisposable
    {
        private readonly IGameEventBus _bus;
        private readonly AudioCatalog _catalog;
        private readonly IAudioPlayback _playback;

        private readonly Dictionary<string, float> _lastPlayTimes = new();
        private readonly List<IDisposable> _subscriptions = new();
        private int _lastRunLevel = -1;

        public AudioService(IGameEventBus bus, AudioCatalog catalog, IAudioPlayback playback)
        {
            _bus = bus;
            _catalog = catalog;
            _playback = playback;

            // —— Мяч ——
            _subscriptions.Add(_bus.Subscribe<BallContactEvent>(OnBallContact));

            // —— Голы ——
            _subscriptions.Add(_bus.Subscribe<GoalScoredEvent>(OnGoalScored));

            // —— Матч ——
            _subscriptions.Add(_bus.Subscribe<MatchStartedEvent>(OnMatchStarted));
            _subscriptions.Add(_bus.Subscribe<MatchEndedEvent>(_ => OnMatchEnded()));
            _subscriptions.Add(_bus.Subscribe<MatchTimeAdjustedEvent>(OnMatchTimeAdjusted));
            _subscriptions.Add(_bus.Subscribe<PitchResetRequestedEvent>(_ => _playback.StopMusic()));

            // —— Защитники ——
            _subscriptions.Add(_bus.Subscribe<DefenderHitEvent>(_ => Play(AudioCatalog.Ids.DefenderHit)));
            _subscriptions.Add(_bus.Subscribe<DefenderDestroyedEvent>(_ => Play(AudioCatalog.Ids.DefenderDestroyed)));
            _subscriptions.Add(_bus.Subscribe<DefenderPromotionStartedEvent>(_ =>
                Play(AudioCatalog.Ids.PromotionStarted)));
            _subscriptions.Add(_bus.Subscribe<DefenderPromotionCompletedEvent>(_ =>
                Play(AudioCatalog.Ids.PromotionCompleted)));
            _subscriptions.Add(_bus.Subscribe<DefenderReturnedHomeEvent>(_ =>
                Play(AudioCatalog.Ids.DefenderReturned)));
            _subscriptions.Add(_bus.Subscribe<DefenderRoleChangedEvent>(OnDefenderRoleChanged));

            // —— Прогрессия забега ——
            _subscriptions.Add(_bus.Subscribe<PerkPickedEvent>(_ => Play(AudioCatalog.Ids.PerkPick)));
            _subscriptions.Add(_bus.Subscribe<RunProgressionUpdatedEvent>(OnRunProgressionUpdated));

            // —— Фазы поля ——
            // BonusPickOpen — только через PitchPhaseChangedEvent (BonusPick), не BonusPickOfferedEvent
            _subscriptions.Add(_bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));

            // —— UI / навигация ——
            _subscriptions.Add(_bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            // События без звука (на шине, но не подписываемся):
            // BallServedEvent — звук старта через MatchStartedEvent
            // BallReturnedToKeeperEvent — дублирует BallContactEvent (PlayerKeeper)
            // BonusPickOfferedEvent — звук через PitchPhaseChangedEvent (BonusPick)
            // MatchTimerChangedEvent, MatchScoreChangedEvent — слишком частые
            // DefenderDamagedEvent — дублирует DefenderHitEvent + BallContactEvent
        }

        private void OnBallContact(BallContactEvent obj)
        {
            if (obj.Kind != BallContactKind.Wall)
            {
                //Здесь пинок мячика
                Play(AudioCatalog.Ids.BallHit);
            }
            else
            {
                Play(AudioCatalog.Ids.BallHit);
            }
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();
        }

        private void OnMatchStarted(MatchStartedEvent _)
        {
            Play(AudioCatalog.Ids.MatchStart);
            Play(AudioCatalog.Ids.MusicMatch);
        }

        private void OnGoalScored(GoalScoredEvent e)
        {
            Play(e.IsPlayerGoal ? AudioCatalog.Ids.GoalScored : AudioCatalog.Ids.GoalConceded);
        }

        private void OnMatchEnded()
        {
            Play(AudioCatalog.Ids.MatchEnd);
            _playback.StopMusic();
        }

        private void OnMatchTimeAdjusted(MatchTimeAdjustedEvent e)
        {
            if (e.DeltaSeconds > 0f)
                Play(AudioCatalog.Ids.TimeBonus);
            else if (e.DeltaSeconds < 0f)
                Play(AudioCatalog.Ids.TimePenalty);
        }

        private void OnDefenderRoleChanged(DefenderRoleChangedEvent e)
        {
            if (e.IsGoalkeeper)
                Play(AudioCatalog.Ids.DefenderRoleChanged);
        }

        private void OnRunProgressionUpdated(RunProgressionUpdatedEvent e)
        {
            if (_lastRunLevel >= 0 && e.RunLevel > _lastRunLevel)
                Play(AudioCatalog.Ids.LevelUp);

            _lastRunLevel = e.RunLevel;
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            switch (e.Phase)
            {
                case PitchPhase.Reshuffle:
                    Play(AudioCatalog.Ids.ReshuffleStart);
                    break;
                case PitchPhase.BonusPick:
                    Play(AudioCatalog.Ids.BonusPickOpen);
                    break;
            }
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            if (e.Current == NavigationState.MainMenu && e.Previous == NavigationState.OnField)
                _playback.PauseMusic();
            else if (e.ResumingPausedMatch)
                _playback.ResumeMusic();

            switch (e.Current)
            {
                case NavigationState.MainMenu when e.Previous != NavigationState.MainMenu:
                    Play(AudioCatalog.Ids.UiMenuOpen);
                    break;
                case NavigationState.Pause when e.Previous != NavigationState.Pause:
                    Play(AudioCatalog.Ids.UiPauseOpen);
                    break;
                case NavigationState.Tournament when e.Previous != NavigationState.Tournament:
                    Play(AudioCatalog.Ids.UiTournamentOpen);
                    break;
            }
        }

        private void Play(string soundId)
        {
            if (!_catalog.TryGet(soundId, out var definition))
                return;

            if (definition.Clips == null || definition.Clips.Length == 0)
                return;

            if (definition.Cooldown > 0f
                && _lastPlayTimes.TryGetValue(soundId, out var lastTime)
                && Time.time - lastTime < definition.Cooldown)
                return;

            if (!definition.AllowOverlap && _playback.IsPlaying(soundId))
                return;

            _playback.Play(definition);
            _lastPlayTimes[soundId] = Time.time;
        }
    }
}
