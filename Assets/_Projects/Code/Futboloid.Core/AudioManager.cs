using System.Collections.Generic;
using UnityEngine;

namespace Futboloid.Core
{
    /// <summary>
    /// Простой аудиоменеджер без DI и шины событий.
    /// Вызывается напрямую из игровых скриптов: AudioManager.Instance.PlayEvent("BallHit");
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Глобальные настройки")]
        [Tooltip("Максимальное количество одновременных звуков (голосов).")]
        [SerializeField] private int maxVoices = 8;

        [Header("Fade")]
        [Tooltip("Глобальный тумблер для Fade In / Fade Out.")]
        [SerializeField] private bool enableFade = true;

        [Header("Music Pause")]
        [Tooltip("Музыка ставится на паузу при выходе в главное меню.")]
        [SerializeField] private bool pauseMusicOnMainMenu = true;

        private List<AudioClipSource> _sources = new List<AudioClipSource>();
        private Dictionary<string, AudioClipSource> _eventMap = new Dictionary<string, AudioClipSource>();
        private AudioClipSource _musicSource;
        private bool _musicWasPlaying;

        public bool EnableFade
        {
            get => enableFade;
            set => enableFade = value;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            var allSources = GetComponentsInChildren<AudioClipSource>();
            foreach (var source in allSources)
            {
                _sources.Add(source);
                if (!string.IsNullOrEmpty(source.eventName))
                {
                    _eventMap[source.eventName] = source;
                }

                if (source.eventName == "MusicStart")
                {
                    _musicSource = source;
                }
            }
        }

        /// <summary>
        /// Воспроизводит звук по имени события.
        /// Вызывать из игровых скриптов: AudioManager.Instance.PlayEvent("BallHit");
        /// </summary>
        public void PlayEvent(string eventName)
        {
            if (!_eventMap.TryGetValue(eventName, out var clipSource))
            {
                Debug.LogWarning($"AudioManager: Нет динамика для события '{eventName}'");
                return;
            }

            if (!clipSource.allowOverlap && clipSource.IsPlaying)
                return;

            if (!CanPlayWithPriority(clipSource))
                return;

            clipSource.PlayRandom();
        }

        /// <summary>
        /// Останавливает звук по имени события (для музыки при рестарте/конце матча).
        /// </summary>
        public void StopEvent(string eventName)
        {
            if (_eventMap.TryGetValue(eventName, out var clipSource))
            {
                clipSource.Stop();
            }
        }

        /// <summary>
        /// Ставит музыку на паузу (при выходе в главное меню).
        /// </summary>
        public void PauseMusic()
        {
            if (_musicSource == null || !_musicSource.source.isPlaying)
                return;

            _musicWasPlaying = true;
            _musicSource.source.Pause();
        }

        /// <summary>
        /// Возобновляет музыку (при возврате в игру).
        /// </summary>
        public void ResumeMusic()
        {
            if (_musicSource == null || !_musicWasPlaying)
                return;

            _musicWasPlaying = false;
            _musicSource.source.Play();
        }

        private bool CanPlayWithPriority(AudioClipSource newSource)
        {
            var playingSources = new List<AudioClipSource>();
            foreach (var s in _sources)
            {
                if (s.source.isPlaying)
                    playingSources.Add(s);
            }

            if (playingSources.Count >= maxVoices)
            {
                AudioClipSource lowestPrioritySource = null;
                int lowestPriority = -1;

                foreach (var s in playingSources)
                {
                    if (lowestPrioritySource == null || s.priority > lowestPriority)
                    {
                        lowestPriority = s.priority;
                        lowestPrioritySource = s;
                    }
                }

                if (lowestPrioritySource != null && newSource.priority < lowestPrioritySource.priority)
                {
                    lowestPrioritySource.Stop();
                }
                else
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}
        
