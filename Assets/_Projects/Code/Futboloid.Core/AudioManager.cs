using System;
using System.Collections.Generic;
using UnityEngine;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;

namespace Futboloid.Core
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Глобальные настройки")]
        [Tooltip("Максимальное количество одновременных звуков (голосов).")]
        [SerializeField] private int maxVoices = 8;

        // Список всех динамиков (детей)
        private List<AudioClipSource> sources = new List<AudioClipSource>();
        
        // Карта для быстрого поиска: "BallHit" -> AudioClipSource
        private Dictionary<string, AudioClipSource> eventMap = new Dictionary<string, AudioClipSource>();

        // Подписки
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly IGameEventBus _bus;

        public AudioManager(IGameEventBus bus)
        {
            _bus = bus;
        }

        private void Awake()
        {
            // Собираем все дочерние объекты с компонентом AudioClipSource
            var allSources = GetComponentsInChildren<AudioClipSource>();
            foreach (var source in allSources)
            {
                sources.Add(source);
                if (!string.IsNullOrEmpty(source.eventName))
                {
                    eventMap[source.eventName] = source;
                }
            }
            
            DontDestroyOnLoad(gameObject);
        }
        
        private void OnEnable()
        {
            if (_bus == null) return;
            
            // Подписываемся на события и мапим их на имена динамиков
            _subscriptions.Add(_bus.Subscribe<GoalScoredEvent>(e => PlayEvent("GoalScored")));
            _subscriptions.Add(_bus.Subscribe<BallReturnedToKeeperEvent>(e => PlayEvent("BallHit")));
            _subscriptions.Add(_bus.Subscribe<BallHitEvent>(e => PlayEvent("BallHit")));
            _subscriptions.Add(_bus.Subscribe<BallServedEvent>(e => PlayEvent("MatchStart")));
            _subscriptions.Add(_bus.Subscribe<MatchEndedEvent>(e => PlayEvent("MatchEnd")));
        }

        private void OnDisable()
        {
            foreach (var sub in _subscriptions)
            {
                sub?.Dispose();
            }
            _subscriptions.Clear();
        }

        /// <summary>
        /// Вызывается при наступлении события.
        /// </summary>
        private void PlayEvent(string eventName)
        {
            if (!eventMap.TryGetValue(eventName, out var clipSource))
            {
                Debug.LogWarning($"AudioManager: Нет динамика для события '{eventName}'");
                return;
            }

            // Если динамик играет и мы превысили лимит голосов — выкидываем самый старый
            if (clipSource.source.isPlaying)
            {
                CheckVoiceLimit();
            }

            // Играем случайный звук из массива этого динамика
            clipSource.PlayRandom();
        }
        
        /// <summary>
        /// Если голосов больше, чем лимит, останавливаем самый старый.
        /// </summary>
        private void CheckVoiceLimit()
        {
            // Собираем все источники, которые сейчас играют (включая вложенные)
            var playingSources = new List<AudioSource>();
            foreach (var s in sources)
            {
                if (s.source.isPlaying)
                {
                    playingSources.Add(s.source);
                }
            }

            // Если превысили лимит
            if (playingSources.Count > maxVoices)
            {
                // Находим самый старый (первый в списке)
                if (playingSources.Count > 0)
                {
                    var oldest = playingSources[0];
                    oldest.Stop();
                }
            }
        }
    }
}
        
