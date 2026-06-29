using UnityEngine;

namespace Futboloid.Gameplay.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class RandomMusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip[] musicClips;

        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.spatialBlend = 0f;
            _audioSource.loop = false;

            if (musicClips.Length > 0)
                PlayRandomMusic();
            else
                Debug.LogWarning("[RandomMusicPlayer] musicClips is empty.", this);
        }

        private void Update()
        {
            if (!_audioSource.isPlaying && musicClips.Length > 0 && _audioSource.clip != null)
                PlayRandomMusic();
        }

        private void PlayRandomMusic()
        {
            if (musicClips.Length == 0)
                return;

            var clip = musicClips[Random.Range(0, musicClips.Length)];
            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
}
