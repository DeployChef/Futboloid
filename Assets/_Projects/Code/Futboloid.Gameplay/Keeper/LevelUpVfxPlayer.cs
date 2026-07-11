using System;
using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Keeper
{
    /// <summary>
    /// Проигрывает VFX-префаб на персонаже при повышении уровня.
    /// Вешается на GameObject вратаря: позиция эффекта = позиция вратаря + локальный оффсет.
    /// Шина событий инжектится через VContainer автоматически (InjectGameObject).
    /// </summary>
    public sealed class LevelUpVfxPlayer : MonoBehaviour
    {
        private const float FallbackLifetime = 3f;
        private const string DefaultSortingLayer = "FX";

        [SerializeField] private GameObject vfxPrefab;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 1f, 0f);
        [SerializeField] private bool followCharacter = true;
        [Tooltip("Секунды до авто-удаления эффекта. <= 0 — определить по длительности ParticleSystem.")]
        [SerializeField] private float destroyAfter = -1f;
        [Tooltip("Сортировочный слой для рендереров частиц, чтобы эффект был поверх спрайтов поля/персонажа.")]
        [SerializeField] private string sortingLayerName = DefaultSortingLayer;
        [Tooltip("Порядок внутри сортировочного слоя. Больше = поверх.")]
        [SerializeField] private int sortingOrder = 10;

        private readonly List<IDisposable> _subscriptions = new();

        private void Awake()
        {
            if (vfxPrefab == null)
                Debug.LogWarning("[LevelUpVfxPlayer] VfxPrefab is not assigned.", this);
        }

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            _subscriptions.Add(bus.Subscribe<LevelUpEvent>(_ => PlayVfx()));
        }

        private void PlayVfx()
        {
            if (vfxPrefab == null)
                return;

            var instance = Instantiate(vfxPrefab, transform);
            instance.transform.localPosition = localOffset;

            if (!followCharacter)
                instance.transform.SetParent(null, worldPositionStays: true);

            PlayAllParticles(instance);
            ApplySortingLayer(instance);
            UseUnscaledTime(instance);

            var lifetime = destroyAfter > 0f
                ? destroyAfter
                : Mathf.Max(GetParticleDuration(instance), FallbackLifetime);

            Destroy(instance, lifetime);
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

        private static float GetParticleDuration(GameObject go)
        {
            var total = 0f;
            var particles = go.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particles)
            {
                if (!ps.main.loop)
                    total = Mathf.Max(total, ps.main.duration);
            }

            return total;
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
