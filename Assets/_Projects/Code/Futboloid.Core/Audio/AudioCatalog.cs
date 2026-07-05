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
            public const string BallHit = "BallHit";
            public const string GoalScored = "GoalScored";
            public const string GoalConceded = "GoalConceded";
            public const string MatchStart = "MatchStart";
            public const string MatchEnd = "MatchEnd";
            public const string MusicMatch = "MusicMatch";
            public const string PerkPick = "PerkPick";
        }

        [SerializeField] private List<SoundDefinition> sounds = new();
        [SerializeField] private int maxSfxVoices = 8;

        public int MaxSfxVoices => maxSfxVoices;

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
