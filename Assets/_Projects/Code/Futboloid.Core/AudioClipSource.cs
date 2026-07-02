using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Futboloid.Core
{
    /// <summary>
    /// Простой динамик. Вешается на дочерние объекты AudioManager.
    /// </summary>
    public class AudioClipSource : MonoBehaviour
    {
        [Header("Настройки")]
        public string eventName;
        public AudioClip[] clips;
        public int priority = 50;
        public bool allowOverlap = true;

        [Header("Кулдаун")]
        [Tooltip("Минимальный интервал между воспроизведениями этого динамика (сек). 0 = без кулдауна.")]
        public float cooldown = 0f;

        [Header("Loop")]
        [Tooltip("Если true, звук будет играть циклично (для музыки).")]
        public bool loop = false;

        [Header("Fade In/Out")]
        [Tooltip("Включить плавное появление/затухание.")]
        public bool enableFade = true;
        [Tooltip("Длительность фейда в секундах.")]
        public float fadeDuration = 1.0f;

        [Header("Микшер")]
        public AudioMixerGroup mixerGroup;

        public AudioSource source;
        public bool IsPlaying => source != null && source.isPlaying;

        private float _lastPlayTime = -999f;
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            if (source == null)
                source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = 0f; // Стартовая громкость для фейд-ина
        }

        public void PlayRandom()
        {
            if (source == null || clips == null || clips.Length == 0) return;

            // Проверка кулдауна
            if (cooldown > 0f && Time.time - _lastPlayTime < cooldown)
                return;

            // Проверка перекрытия (Overlap)
            if (!allowOverlap && IsPlaying)
                return;

            // Назначаем группу микшера
            if (mixerGroup != null)
                source.outputAudioMixerGroup = mixerGroup;

            // Устанавливаем loop для музыки
            source.loop = loop;

            int index = UnityEngine.Random.Range(0, clips.Length);
            source.clip = clips[index];

            // Запускаем фейд ин если включено
            if (enableFade && AudioManager.Instance != null && AudioManager.Instance.EnableFade)
                StartCoroutine(FadeIn());
            else
                source.volume = 1f;

            source.Play();
            _lastPlayTime = Time.time;
        }

        public void Stop()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            if (enableFade && AudioManager.Instance != null && AudioManager.Instance.EnableFade)
                StartCoroutine(FadeOut());
            else
            {
                source.volume = 0f;
                source.Stop();
            }
        }

        private IEnumerator FadeIn()
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeTo(1f, fadeDuration));
            yield return _fadeRoutine;
            _fadeRoutine = null;
        }

        private IEnumerator FadeOut()
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeTo(0f, fadeDuration));
            yield return _fadeRoutine;
            _fadeRoutine = null;
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            float start = source.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            source.volume = target;
            if (target == 0f)
                source.Stop();
        }
    }
}
