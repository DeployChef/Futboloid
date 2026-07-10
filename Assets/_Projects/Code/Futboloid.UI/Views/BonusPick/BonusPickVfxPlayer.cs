using System;
using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;
using VContainer;

namespace Futboloid.UI.Views.BonusPick
{
    /// <summary>
    /// VFX-фон позади карточек выбора перка. Спавнится в мире на позиции Spawn Anchor.
    /// Стартует при появлении карточек (BonusPickOfferedEvent),
    /// останавливается при выборе перка (PerkPickedEvent).
    /// useUnscaledTime — эффект не замирает во время паузы (timeScale = 0).
    /// </summary>
    public sealed class BonusPickVfxPlayer : MonoBehaviour
    {
        [SerializeField] private GameObject vfxPrefab;
        [Tooltip("Объект на сцене, позиция которого используется для спавна VFX в мире.")]
        [SerializeField] private Transform spawnAnchor;
        [Tooltip("Если true — VFX двигается вместе с якорем. Если false — спавн в точке якоря, без привязки.")]
        [SerializeField] private bool followAnchor = true;
        [Tooltip("Сортировочный слой для рендереров частиц.")]
        [SerializeField] private string sortingLayerName = "FX";
        [Tooltip("Порядок внутри слоя. Меньше = позади карточек.")]
        [SerializeField] private int sortingOrder = 0;

        private readonly List<IDisposable> _subscriptions = new();
        private GameObject _currentInstance;

        private void Awake()
        {
            if (vfxPrefab == null)
                Debug.LogWarning("[BonusPickVfxPlayer] VfxPrefab is not assigned.", this);
        }

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            _subscriptions.Add(bus.Subscribe<BonusPickOfferedEvent>(_ => PlayVfx()));
            _subscriptions.Add(bus.Subscribe<PerkPickedEvent>(_ => StopVfx()));
        }

        private void PlayVfx()
        {
            if (vfxPrefab == null)
                return;

            StopVfx();

            if (spawnAnchor == null)
            {
                Debug.LogWarning("[BonusPickVfxPlayer] SpawnAnchor is not assigned.", this);
                return;
            }

            _currentInstance = Instantiate(vfxPrefab, followAnchor ? spawnAnchor : null);
            _currentInstance.transform.position = spawnAnchor.position;
            _currentInstance.transform.rotation = spawnAnchor.rotation;

            PlayAllParticles(_currentInstance);
            ApplySortingLayer(_currentInstance);
            UseUnscaledTime(_currentInstance);
        }

        private void StopVfx()
        {
            if (_currentInstance == null)
                return;

            Destroy(_currentInstance);
            _currentInstance = null;
        }

        private static void PlayAllParticles(GameObject go)
        {
            var particles = go.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particles)
            {
                if (ps != null && !ps.isPlaying)
                    ps.Play(true);
            }
        }

        private void ApplySortingLayer(GameObject go)
        {
            if (string.IsNullOrEmpty(sortingLayerName))
                return;

            var renderers = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = sortingLayerName;
                r.sortingOrder = sortingOrder;
            }
        }

        private static void UseUnscaledTime(GameObject go)
        {
            var particles = go.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particles)
            {
                var main = ps.main;
                main.useUnscaledTime = true;
            }
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
