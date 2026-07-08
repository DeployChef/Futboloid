using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.StatusEffects;

namespace Futboloid.Core.Audio
{
    /// <summary>
    /// Слушает шину и передаёт Sound Id в <see cref="IAudioManager"/>.
    /// Полный маппинг событий — в Wiki: «Каталог событий и звуков».
    /// </summary>
    public sealed class AudioService : IDisposable
    {
        private readonly IGameEventBus _bus;
        private readonly IAudioManager _audio;

        private readonly List<IDisposable> _subscriptions = new();
        private int _lastRunLevel = -1;

        public AudioService(IGameEventBus bus, IAudioManager audio)
        {
            _bus = bus;
            _audio = audio;

            // —— Мяч ——
            _subscriptions.Add(_bus.Subscribe<BallContactEvent>(OnBallContact));

            // —— Голы ——
            _subscriptions.Add(_bus.Subscribe<GoalScoredEvent>(OnGoalScored));

            // —— Матч ——
            _subscriptions.Add(_bus.Subscribe<MatchStartedEvent>(OnMatchStarted));
            _subscriptions.Add(_bus.Subscribe<MatchEndedEvent>(OnMatchEnded));
            _subscriptions.Add(_bus.Subscribe<MatchTimeAdjustedEvent>(OnMatchTimeAdjusted));
            _subscriptions.Add(_bus.Subscribe<PitchResetRequestedEvent>(OnPitchResetRequested));

            // —— Защитники ——
            _subscriptions.Add(_bus.Subscribe<DefenderHitEvent>(OnDefenderHit));
            _subscriptions.Add(_bus.Subscribe<DefenderDestroyedEvent>(OnDefenderDestroyed));
            _subscriptions.Add(_bus.Subscribe<DefenderPromotionStartedEvent>(OnDefenderPromotionStarted));
            _subscriptions.Add(_bus.Subscribe<DefenderPromotionCompletedEvent>(OnDefenderPromotionCompleted));
            _subscriptions.Add(_bus.Subscribe<DefenderReturnedHomeEvent>(OnDefenderReturnedHome));
            _subscriptions.Add(_bus.Subscribe<DefenderRoleChangedEvent>(OnDefenderRoleChanged));

            // —— Прогрессия забега ——
            _subscriptions.Add(_bus.Subscribe<PerkPickedEvent>(OnPerkPicked));
            _subscriptions.Add(_bus.Subscribe<RunProgressionUpdatedEvent>(OnRunProgressionUpdated));

            // —— Комбо / очки ——
            _subscriptions.Add(_bus.Subscribe<ComboScoreChangedEvent>(OnComboScoreChanged));

            // —— Баффы / дебаффы ——
            _subscriptions.Add(_bus.Subscribe<StatusEffectAppliedEvent>(OnStatusEffectApplied));
            _subscriptions.Add(_bus.Subscribe<StatusEffectRemovedEvent>(OnStatusEffectRemoved));

            // —— Фазы поля ——
            _subscriptions.Add(_bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));

            // —— UI / навигация ——
            _subscriptions.Add(_bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();
        }

        private void OnBallContact(BallContactEvent e)
        {
            switch (e.Kind)
            {
                case BallContactKind.Wall:
                    _audio.Play(AudioCatalog.Ids.BallHit);
                    break;
                case BallContactKind.PlayerKeeper:
                case BallContactKind.Defender:
                    _audio.Play(AudioCatalog.Ids.BallHitMan);
                    break;
            }
        }

        private void OnGoalScored(GoalScoredEvent e) =>
            _audio.Play(e.IsPlayerGoal ? AudioCatalog.Ids.GoalScored : AudioCatalog.Ids.GoalConceded);

        private void OnMatchStarted(MatchStartedEvent _) =>
            _audio.Play(AudioCatalog.Ids.MatchStart);

        private void OnMatchEnded(MatchEndedEvent _)
        {
            _audio.Play(AudioCatalog.Ids.MatchEnd);
            _audio.StopMusic();
        }

        private void OnMatchTimeAdjusted(MatchTimeAdjustedEvent e)
        {
            if (e.DeltaSeconds > 0f)
                _audio.Play(AudioCatalog.Ids.TimeBonus);
            else if (e.DeltaSeconds < 0f)
                _audio.Play(AudioCatalog.Ids.TimePenalty);
        }

        private void OnPitchResetRequested(PitchResetRequestedEvent _) => _audio.StopMusic();

        private void OnDefenderHit(DefenderHitEvent _) => _audio.Play(AudioCatalog.Ids.DefenderHit);

        private void OnDefenderDestroyed(DefenderDestroyedEvent _) =>
            _audio.Play(AudioCatalog.Ids.DefenderDestroyed);

        private void OnDefenderPromotionStarted(DefenderPromotionStartedEvent _) =>
            _audio.Play(AudioCatalog.Ids.PromotionStarted);

        private void OnDefenderPromotionCompleted(DefenderPromotionCompletedEvent _) =>
            _audio.Play(AudioCatalog.Ids.PromotionCompleted);

        private void OnDefenderReturnedHome(DefenderReturnedHomeEvent _) =>
            _audio.Play(AudioCatalog.Ids.DefenderReturned);

        private void OnDefenderRoleChanged(DefenderRoleChangedEvent e)
        {
            if (e.IsGoalkeeper)
                _audio.Play(AudioCatalog.Ids.DefenderRoleChanged);
        }

        private void OnPerkPicked(PerkPickedEvent _) => _audio.Play(AudioCatalog.Ids.PerkPick);

        private void OnRunProgressionUpdated(RunProgressionUpdatedEvent e)
        {
            if (_lastRunLevel >= 0 && e.RunLevel > _lastRunLevel)
                _audio.Play(AudioCatalog.Ids.LevelUp);

            _lastRunLevel = e.RunLevel;
        }

        private void OnComboScoreChanged(ComboScoreChangedEvent e)
        {
            if (e.Multiplier > e.PreviousMultiplier)
            {
                _audio.Play(AudioCatalog.Ids.ComboMultiplierUp);
                return;
            }

            // Decay шагает по -1 — не спамим звуком; сброс вратарём обычно -3 и больше.
            if (e.Multiplier < e.PreviousMultiplier
                && e.PreviousMultiplier - e.Multiplier >= 2)
            {
                _audio.Play(AudioCatalog.Ids.ComboMultiplierDown);
                return;
            }

            if (e.DeltaPoints > 0)
                _audio.Play(AudioCatalog.Ids.ScorePoints);
        }

        private void OnStatusEffectApplied(StatusEffectAppliedEvent e)
        {
            _audio.Play(e.IsDebuff ? AudioCatalog.Ids.DebuffApplied : AudioCatalog.Ids.BuffApplied);
        }

        private void OnStatusEffectRemoved(StatusEffectRemovedEvent e)
        {
            if (e.Reason == StatusEffectRemoveReason.Consumed)
                _audio.Play(AudioCatalog.Ids.BuffConsumed);
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            switch (e.Phase)
            {
                case PitchPhase.Reshuffle:
                    _audio.Play(AudioCatalog.Ids.ReshuffleStart);
                    break;
                case PitchPhase.BonusPick:
                    _audio.Play(AudioCatalog.Ids.BonusPickOpen);
                    break;
            }
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            if (ShouldPauseMusic(e))
                _audio.PauseMusic();
            else if (e.Current == NavigationState.OnField)
                HandleFieldMusic(e);

            switch (e.Current)
            {
                case NavigationState.MainMenu when e.Previous != NavigationState.MainMenu:
                    _audio.Play(AudioCatalog.Ids.UiMenuOpen);
                    break;
                case NavigationState.Pause when e.Previous != NavigationState.Pause:
                    _audio.Play(AudioCatalog.Ids.UiPauseOpen);
                    break;
                case NavigationState.Tournament when e.Previous != NavigationState.Tournament:
                    _audio.Play(AudioCatalog.Ids.UiTournamentOpen);
                    break;
            }
        }

        private static bool ShouldPauseMusic(NavigationChangedEvent e) =>
            e.Current switch
            {
                NavigationState.Pause when e.Previous == NavigationState.OnField => true,
                NavigationState.MainMenu when e.Previous is NavigationState.OnField or NavigationState.Pause => true,
                _ => false
            };

        private void HandleFieldMusic(NavigationChangedEvent e)
        {
            if (ShouldResumeMusic(e))
            {
                _audio.ResumeMusic();
                return;
            }

            if (!_audio.IsPlaying(AudioCatalog.Ids.MusicMatch))
                _audio.Play(AudioCatalog.Ids.MusicMatch);
        }

        private bool ShouldResumeMusic(NavigationChangedEvent e)
        {
            if (e.Previous == NavigationState.Pause || e.ResumingPausedMatch)
                return true;

            return e.Previous == NavigationState.MainMenu && _audio.IsMusicPaused;
        }
    }
}
