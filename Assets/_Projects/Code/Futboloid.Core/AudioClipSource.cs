using UnityEngine;
using UnityEngine.Audio;

namespace Futboloid.Core
{
    /// <summary>
    /// Компонент-динамик. Содержит AudioSource и массив звуков для конкретного события.
    /// Вешается на дочерние объекты AudioManager.
    /// </summary>
    public class AudioClipSource : MonoBehaviour
    {
        [Header("Настройки динамика")]
        [Tooltip("Имя события в шине, которое запускает этот динамик (например, 'BallHit').")]
        public string eventName;

        [Tooltip("Массив звуков. При срабатывании выбирается случайный.")]
        public AudioClip[] clips;

        [Tooltip("Группа AudioMixer для этого динамика.")]
        public AudioMixerGroup mixerGroup;

        public AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }
            source.playOnAwake = false;
        }

        /// <summary>
        /// Проигрывает случайный звук из массива.
        /// </summary>
        public void PlayRandom()
        {
            if (source == null || clips == null || clips.Length == 0) return;

            // Назначаем микшер
            if (mixerGroup != null)
            {
                source.outputAudioMixerGroup = mixerGroup;
            }

            // Выбираем случайный звук
            int index = Random.Range(0, clips.Length);
            source.clip = clips[index];
            
            // Воспроизводим
            source.Play();
        }

        /// <summary>
        /// Остановить этот динамик (используется для выкидывания старых голосов).
        /// </summary>
        public void Stop()
        {
            source?.Stop();
        }
    }
}
