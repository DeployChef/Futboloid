using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>
    /// Spawns a death VFX prefab at the defender's position when it dies.
    /// The instance is detached from the defender so it survives the defender's Destroy.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DefenderDeathVfx : MonoBehaviour
    {
        private const float FallbackLifetime = 3f;
        private const string DefaultSortingLayer = "FX";

        [SerializeField] private GameObject vfxPrefab;
        [SerializeField] private Vector3 localOffset = Vector3.zero;
        [Tooltip("Секунды до авто-удаления эффекта. <= 0 — определить по длительности ParticleSystem.")]
        [SerializeField] private float destroyAfter = -1f;
        [Tooltip("Сортировочный слой для рендереров частиц, чтобы эффект был поверх спрайтов поля/персонажа.")]
        [SerializeField] private string sortingLayerName = DefaultSortingLayer;
        [Tooltip("Порядок внутри сортировочного слоя. Больше = поверх.")]
        [SerializeField] private int sortingOrder = 10;

        public bool HasPrefab => vfxPrefab != null;

        /// <summary>
        /// Spawns the VFX at the current position, detached from this object.
        /// Safe to call right before the defender is destroyed.
        /// </summary>
        public void Play()
        {
            if (vfxPrefab == null)
                return;

            var worldPos = transform.position + localOffset;
            var instance = Instantiate(vfxPrefab, worldPos, Quaternion.identity, null);

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
    }
}
