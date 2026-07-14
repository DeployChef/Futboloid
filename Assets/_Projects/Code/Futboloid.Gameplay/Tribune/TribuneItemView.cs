using DG.Tweening;
using Futboloid.Core.StatusEffects;
using Futboloid.Gameplay.Keeper;
using UnityEngine;

namespace Futboloid.Gameplay.Tribune
{
    /// <summary>
    /// Предмет с трибуны: полёт по дуге (DOTween), при касании вратаря — timed-эффект.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TribuneItemView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D _body;
        private StatusEffectDefinition _definition;
        private IStatusEffectService _statusEffects;
        private Vector2 _start;
        private Vector2 _control;
        private Vector2 _end;
        private float _progress;
        private bool _consumed;
        private bool _initialized;
        private Tween _flightTween;
        private Tween _spinTween;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            _body = GetComponent<Rigidbody2D>();
            if (_body == null)
                _body = gameObject.AddComponent<Rigidbody2D>();

            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.gravityScale = 0f;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.isTrigger = true;
        }

        public void Initialize(
            StatusEffectDefinition definition,
            IStatusEffectService statusEffects,
            Vector2 start,
            Vector2 end,
            float arcHeight,
            float durationSeconds)
        {
            _definition = definition;
            _statusEffects = statusEffects;
            _start = start;
            _end = end;
            _control = new Vector2(
                (start.x + end.x) * 0.5f,
                start.y + arcHeight);
            _progress = 0f;
            _consumed = false;
            _initialized = true;

            if (spriteRenderer != null)
            {
                if (definition?.Icon != null)
                    spriteRenderer.sprite = definition.Icon;

                spriteRenderer.sortingOrder = 50;
            }

            transform.rotation = Quaternion.identity;
            SetPosition(start);

            var duration = Mathf.Max(0.1f, durationSeconds);
            StartFlight(duration);
            StartSpin(duration);
        }

        private void StartFlight(float durationSeconds)
        {
            KillFlightTween();

            _flightTween = DOTween.To(
                    () => _progress,
                    value =>
                    {
                        _progress = value;
                        SetPosition(EvaluateBezier(_start, _control, _end, _progress));
                    },
                    1f,
                    durationSeconds)
                .SetEase(Ease.Linear)
                .SetTarget(this)
                .OnComplete(OnFlightComplete);
        }

        private void StartSpin(float durationSeconds)
        {
            KillSpinTween();

            var spinDirection = Random.value < 0.5f ? -1f : 1f;
            var spinDegrees = spinDirection * Random.Range(360f, 720f);

            _spinTween = transform
                .DORotate(new Vector3(0f, 0f, spinDegrees), durationSeconds, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetTarget(this);
        }

        private void OnFlightComplete()
        {
            if (!_consumed)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other) => TryCatch(other);

        private void TryCatch(Collider2D other)
        {
            if (!_initialized || _consumed)
                return;

            if (other.GetComponentInParent<GoalkeeperView>() == null)
                return;

            Consume();
        }

        private void Consume()
        {
            _consumed = true;
            KillTweens();
            _statusEffects?.Apply(_definition);
            Destroy(gameObject);
        }

        private void SetPosition(Vector2 position)
        {
            if (_body != null)
                _body.position = position;
            else
                transform.position = position;
        }

        private void KillTweens()
        {
            KillFlightTween();
            KillSpinTween();
        }

        private void KillFlightTween()
        {
            if (_flightTween != null && _flightTween.IsActive())
                _flightTween.Kill();

            _flightTween = null;
        }

        private void KillSpinTween()
        {
            if (_spinTween != null && _spinTween.IsActive())
                _spinTween.Kill();

            _spinTween = null;
        }

        private void OnDestroy() => KillTweens();

        private static Vector2 EvaluateBezier(Vector2 start, Vector2 control, Vector2 end, float t)
        {
            var u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }
    }
}
