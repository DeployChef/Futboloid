namespace Futboloid.Core.Audio
{
    public interface IAudioPlayback
    {
        void Play(SoundDefinition sound);
        void Stop(string soundId);
        void StopMusic();
        void PauseMusic();
        void ResumeMusic();
        void StopAll();
        bool IsPlaying(string soundId);
        bool TryStopLowestPrioritySfx(int incomingPriority);
    }
}
