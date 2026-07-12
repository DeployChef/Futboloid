using UnityEngine;

namespace Futboloid.Gameplay.Ball
{
    /// <summary>
    /// Визуал огненной стадии: плавно включает CFXR Fire и разворачивает его против направления полёта.
    /// Состояние приходит извне через Sync — компонент не знает о BallView.
    /// </summary>
    public class BallFireVfxView : MonoBehaviour
    {
        [SerializeField] private GameObject fireVfxRoot;
        [SerializeField] private float directionRotationOffset = -90f;
        [SerializeField] private float rotationSmoothTime = 0.12f;

        private ParticleSystem[] _particles;
        private float[] _baseEmissionRates;
        private float _intensity;
        private float _currentAngle;
        private float _rotationVelocity;

        private void Awake()
        {
            if (fireVfxRoot == null)
                fireVfxRoot = FindFireRoot(transform);

            CacheParticles();
            ExtinguishImmediate();
        }

        private void OnDisable() => ExtinguishImmediate();

        public void Sync(bool shouldBurn, Vector2 direction, float fadeSpeed)
        {
            var target = shouldBurn ? 1f : 0f;
            _intensity = Mathf.MoveTowards(_intensity, target, fadeSpeed * Time.deltaTime);
            ApplyIntensity();

            if (_intensity > 0.001f)
                UpdateVfxRotation(direction);
        }

        public void ExtinguishImmediate()
        {
            _intensity = 0f;
            _currentAngle = 0f;
            _rotationVelocity = 0f;

            if (fireVfxRoot == null)
                return;

            SetEmissionEnabled(false);

            for (var i = 0; i < _particles.Length; i++)
            {
                var particle = _particles[i];
                if (particle == null)
                    continue;

                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particle.Clear(true);
            }

            fireVfxRoot.transform.localRotation = Quaternion.identity;
            fireVfxRoot.SetActive(false);
        }

        private void UpdateVfxRotation(Vector2 direction)
        {
            if (fireVfxRoot == null)
                return;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            var targetAngle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg + directionRotationOffset;
            _currentAngle = Mathf.SmoothDampAngle(
                _currentAngle,
                targetAngle,
                ref _rotationVelocity,
                rotationSmoothTime);

            fireVfxRoot.transform.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
        }

        private void ApplyIntensity()
        {
            if (fireVfxRoot == null)
                return;

            var visible = _intensity > 0.001f;

            if (visible)
            {
                if (!fireVfxRoot.activeSelf)
                {
                    fireVfxRoot.SetActive(true);
                    PlayParticles();
                }

                SetEmissionEnabled(true);
                SetEmissionRates(_intensity);
                return;
            }

            SetEmissionRates(0f);
            SetEmissionEnabled(false);

            for (var i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] != null)
                    _particles[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if (_intensity > 0f)
                return;

            fireVfxRoot.SetActive(false);
            _currentAngle = 0f;
            _rotationVelocity = 0f;
            fireVfxRoot.transform.localRotation = Quaternion.identity;
        }

        private void CacheParticles()
        {
            if (fireVfxRoot == null)
            {
                _particles = System.Array.Empty<ParticleSystem>();
                _baseEmissionRates = System.Array.Empty<float>();
                return;
            }

            _particles = fireVfxRoot.GetComponentsInChildren<ParticleSystem>(true);
            _baseEmissionRates = new float[_particles.Length];

            for (var i = 0; i < _particles.Length; i++)
            {
                var emission = _particles[i].emission;
                _baseEmissionRates[i] = emission.rateOverTime.constantMax > 0f
                    ? emission.rateOverTime.constantMax
                    : emission.rateOverTime.constant;
            }
        }

        private void SetEmissionEnabled(bool enabled)
        {
            for (var i = 0; i < _particles.Length; i++)
            {
                var particle = _particles[i];
                if (particle == null)
                    continue;

                var emission = particle.emission;
                emission.enabled = enabled;
            }
        }

        private void SetEmissionRates(float intensity)
        {
            for (var i = 0; i < _particles.Length; i++)
            {
                var particle = _particles[i];
                if (particle == null)
                    continue;

                var emission = particle.emission;
                var rate = emission.rateOverTime;
                rate.constant = _baseEmissionRates[i] * intensity;
                rate.constantMax = _baseEmissionRates[i] * intensity;
                emission.rateOverTime = rate;
            }
        }

        private void PlayParticles()
        {
            for (var i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] != null && !_particles[i].isPlaying)
                    _particles[i].Play(true);
            }
        }

        private static GameObject FindFireRoot(Transform ballRoot)
        {
            for (var i = 0; i < ballRoot.childCount; i++)
            {
                var child = ballRoot.GetChild(i);
                if (child.name.Contains("CFXR Fire"))
                    return child.gameObject;
            }

            return null;
        }
    }
}
