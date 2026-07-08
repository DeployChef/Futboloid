using System.Collections.Generic;
using UnityEngine;

namespace Futboloid.Core.Audio
{
    [CreateAssetMenu(fileName = "AudioCatalog", menuName = "Futboloid/Audio Catalog")]
    public class AudioCatalog : ScriptableObject
    {
        public const string ResourcePath = "Data/Settings/AudioCatalog";

        public static class Ids
        {
            // Мяч
            public const string BallHit = "BallHit";
            public const string BallHitMan = "BallHitMan";

            // Голы
            public const string GoalScored = "GoalScored";
            public const string GoalConceded = "GoalConceded";

            // Матч
            public const string MatchStart = "MatchStart";
            public const string MatchEnd = "MatchEnd";
            public const string MusicMatch = "MusicMatch";
            public const string TimeBonus = "TimeBonus";
            public const string TimePenalty = "TimePenalty";

            // Защитники
            public const string DefenderHit = "DefenderHit";
            public const string DefenderDestroyed = "DefenderDestroyed";
            public const string PromotionStarted = "PromotionStarted";
            public const string PromotionCompleted = "PromotionCompleted";
            public const string DefenderReturned = "DefenderReturned";
            public const string DefenderRoleChanged = "DefenderRoleChanged";

            // Прогрессия забега
            public const string PerkPick = "PerkPick";
            public const string BonusPickOpen = "BonusPickOpen";
            public const string LevelUp = "LevelUp";

            // Фазы поля
            public const string ReshuffleStart = "ReshuffleStart";

            // Комбо / очки
            public const string ScorePoints = "ScorePoints";
            public const string ComboMultiplierUp = "ComboMultiplierUp";
            public const string ComboMultiplierDown = "ComboMultiplierDown";

            // Баффы / дебаффы
            public const string BuffApplied = "BuffApplied";
            public const string DebuffApplied = "DebuffApplied";
            public const string BuffConsumed = "BuffConsumed";

            // UI / навигация
            public const string UiMenuOpen = "UiMenuOpen";
            public const string UiPauseOpen = "UiPauseOpen";
            public const string UiTournamentOpen = "UiTournamentOpen";
        }

        [SerializeField] private List<SoundDefinition> sounds = new();

        public IEnumerable<AudioClip> EnumerateClips()
        {
            foreach (var sound in sounds)
            {
                if (sound?.Clips == null)
                    continue;

                foreach (var clip in sound.Clips)
                {
                    if (clip != null)
                        yield return clip;
                }
            }
        }

        /// <summary>
        /// Декодирует клип в память до первого Play(). Иначе Unity делает это синхронно на кадре воспроизведения.
        /// </summary>
        public void WarmupClip(AudioClip clip)
        {
            if (clip == null || clip.loadState != AudioDataLoadState.Unloaded)
                return;

            clip.LoadAudioData();
        }

        public bool TryGet(string id, out SoundDefinition definition)
        {
            foreach (var sound in sounds)
            {
                if (sound != null && sound.Id == id)
                {
                    definition = sound;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public static AudioCatalog Load()
        {
            var catalog = Resources.Load<AudioCatalog>(ResourcePath);
            if (catalog != null)
                return catalog;

            Debug.LogWarning(
                $"[AudioCatalog] Asset not found at Resources/{ResourcePath}. " +
                "Create via Assets → Create → Futboloid → Audio Catalog.");
            return CreateInstance<AudioCatalog>();
        }
    }
}
