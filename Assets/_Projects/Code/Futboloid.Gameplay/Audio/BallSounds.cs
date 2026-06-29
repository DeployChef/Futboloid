using UnityEngine;

namespace Futboloid.Gameplay.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class BallSounds : MonoBehaviour
    {
        [SerializeField] private AudioClip[] impactClips;
        [SerializeField] private float cooldown = 0.5f;

        private AudioSource _audioSource;
        private float _lastPlayTime = -999f;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.spatialBlend = 0f;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Time.time - _lastPlayTime < cooldown)
                return;

            PlayRandomImpactSound();
        }

        public void PlayRandomImpactSound()
        {
            if (impactClips == null || impactClips.Length == 0)
            {
                Debug.LogWarning("[BallSounds] impactClips is empty.", this);
                return;
            }

            var clip = impactClips[Random.Range(0, impactClips.Length)];
            _audioSource.pitch = Random.Range(0.95f, 1.05f);
            _audioSource.PlayOneShot(clip);
            _lastPlayTime = Time.time;
        }
    }
}
