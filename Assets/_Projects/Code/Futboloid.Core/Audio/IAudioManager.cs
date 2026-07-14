using UnityEngine;

namespace Futboloid.Core.Audio
{
    public interface IAudioManager
    {
        void Play(string soundId, float? pitch = null, float? pitchRandomRange = null);
        void Stop(string soundId);
        void StopMusic();
        void PauseMusic();
        void ResumeMusic();
        void StopAll();
        bool IsPlaying(string soundId);
        bool IsMusicPaused { get; }
        bool HasMusicClip { get; }
        System.Collections.Generic.IEnumerable<AudioClip> EnumerateClips();
        void WarmupClip(AudioClip clip);

        /// <summary>Установить громкость музыки (0..1). Сохраняется в PlayerPrefs.</summary>
        void SetMusicVolume(float volume);
        /// <summary>Установить громкость SFX (0..1). Сохраняется в PlayerPrefs.</summary>
        void SetSfxVolume(float volume);
        /// <summary>Текущая громкость музыки (0..1).</summary>
        float GetMusicVolume();
        /// <summary>Текущая громкость SFX (0..1).</summary>
        float GetSfxVolume();
    }
}
