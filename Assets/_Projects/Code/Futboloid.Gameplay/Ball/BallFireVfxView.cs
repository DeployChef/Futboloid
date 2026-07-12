using Futboloid.Core;
using UnityEngine;

namespace Futboloid.Gameplay.Ball
{
    /// <summary>
    /// Огненная стадия мяча: включает дочерние ParticleSystem при высокой скорости, плавно гасит при замедлении.
    /// </summary>
    public class BallFireVfxView : MonoBehaviour
    {
        [SerializeField] private BallView ballView;
        [SerializeField] private ParticleSystem[] fireParticles = System.Array.Empty<ParticleSystem>();

        private float[] _baseEmissionRates = System.Array.Empty<float>();
        private float _displayedIntensity;
        private bool _simulating;

        private void Awake()
        {
            if (ballView == null)
                ballView = GetComponentInParent<BallView>();

            CacheBaseEmissionRates();
            ApplyIntensity(0f);
        }

        private void OnEnable()
        {
            if (ballView == null)
                return;

            ballView.PhaseChanged += OnPhaseChanged;
            _simulating = ballView.IsSimulating;
        }

        private void OnDisable()
        {
            if (ballView != null)
                ballView.PhaseChanged -= OnPhaseChanged;

            _displayedIntensity = 0f;
            ApplyIntensity(0f);
        }

        private void OnPhaseChanged(PitchPhase phase) => _simulating = phase == PitchPhase.Simulating;

        private void Update()
        {
            if (ballView == null)
                return;

            var targetIntensity = _simulating && ballView.InPlay && ballView.IsOnFire ? 1f : 0f;
            var fadeSpeed = ballView.Settings.FireVfxFadeSpeed;
            _displayedIntensity = targetIntensity > _displayedIntensity
                ? Mathf.MoveTowards(_displayedIntensity, targetIntensity, fadeSpeed * Time.deltaTime)
                : Mathf.MoveTowards(_displayedIntensity, targetIntensity, fadeSpeed * 0.65f * Time.deltaTime);

            ApplyIntensity(_displayedIntensity);
        }

        private void CacheBaseEmissionRates()
        {
            if (fireParticles == null || fireParticles.Length == 0)
            {
                _baseEmissionRates = System.Array.Empty<float>();
                return;
            }

            _baseEmissionRates = new float[fireParticles.Length];
            for (var i = 0; i < fireParticles.Length; i++)
            {
                if (fireParticles[i] == null)
                    continue;

                _baseEmissionRates[i] = fireParticles[i].emission.rateOverTime.constant;
            }
        }

        private void ApplyIntensity(float intensity)
        {
            if (fireParticles == null)
                return;

            for (var i = 0; i < fireParticles.Length; i++)
            {
                var particles = fireParticles[i];
                if (particles == null)
                    continue;

                var emission = particles.emission;
                var active = intensity > 0.01f;
                emission.enabled = active;

                if (_baseEmissionRates.Length > i)
                    emission.rateOverTime = _baseEmissionRates[i] * intensity;

                if (active && !particles.isPlaying)
                    particles.Play(true);
                else if (!active && particles.isPlaying)
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
