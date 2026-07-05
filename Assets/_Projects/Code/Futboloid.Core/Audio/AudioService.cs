using System;
using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;

namespace Futboloid.Core.Audio
{
    public sealed class AudioService : IDisposable
    {
        private readonly IGameEventBus _bus;
        private readonly AudioCatalog _catalog;
        private readonly IAudioPlayback _playback;

        private readonly Dictionary<string, float> _lastPlayTimes = new();
        private readonly List<IDisposable> _subscriptions = new();

        public AudioService(IGameEventBus bus, AudioCatalog catalog, IAudioPlayback playback)
        {
            _bus = bus;
            _catalog = catalog;
            _playback = playback;

            _subscriptions.Add(_bus.Subscribe<BallContactEvent>(OnBallContact));
            _subscriptions.Add(_bus.Subscribe<GoalScoredEvent>(OnGoalScored));
            _subscriptions.Add(_bus.Subscribe<MatchStartedEvent>(OnMatchStarted));
            _subscriptions.Add(_bus.Subscribe<MatchEndedEvent>(_ => OnMatchEnded()));
            _subscriptions.Add(_bus.Subscribe<PitchResetRequestedEvent>(_ => _playback.StopMusic()));
            _subscriptions.Add(_bus.Subscribe<PerkPickedEvent>(_ => Play(AudioCatalog.Ids.PerkPick)));
            _subscriptions.Add(_bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();
        }

        private void OnBallContact(BallContactEvent _)
        {
            Play(AudioCatalog.Ids.BallHit);
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

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            if (e.Current == NavigationState.MainMenu && e.Previous == NavigationState.OnField)
                _playback.PauseMusic();
            else if (e.ResumingPausedMatch)
                _playback.ResumeMusic();
        }

        private void Play(string soundId)
        {
            if (!_catalog.TryGet(soundId, out var definition))
            {
                Debug.LogWarning($"[AudioService] Unknown sound id '{soundId}'.");
                return;
            }

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
