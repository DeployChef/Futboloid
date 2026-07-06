using Futboloid.Core.StatusEffects;
using Futboloid.Gameplay.Keeper;
using UnityEngine;

namespace Futboloid.Gameplay.Tribune
{
    /// <summary>
    /// Предмет с трибуны: полёт по дуге-снаряду, при касании вратаря — timed-эффект.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TribuneItemView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D _body;
        private GoalkeeperView _goalkeeper;
        private StatusEffectDefinition _definition;
        private IStatusEffectService _statusEffects;
        private Vector2 _start;
        private Vector2 _control;
        private Vector2 _end;
        private float _duration;
        private float _catchRadius;
        private float _elapsed;
        private bool _consumed;
        private bool _initialized;

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
            GoalkeeperView goalkeeper,
            Vector2 start,
            Vector2 end,
            float launchHeight,
            float launchHorizontalBias,
            float visualScale,
            float catchRadius,
            float durationSeconds)
        {
            _definition = definition;
            _statusEffects = statusEffects;
            _goalkeeper = goalkeeper;
            _start = start;
            _end = end;
            _control = new Vector2(
                Mathf.Lerp(start.x, end.x, launchHorizontalBias),
                start.y + launchHeight);
            _catchRadius = catchRadius;
            _duration = Mathf.Max(0.1f, durationSeconds);
            _elapsed = 0f;
            _consumed = false;
            _initialized = true;

            transform.localScale = Vector3.one;

            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localScale = Vector3.one * visualScale;

                if (definition?.Icon != null)
                    spriteRenderer.sprite = definition.Icon;

                spriteRenderer.sortingOrder = 50;
            }

            if (_body != null)
                _body.position = start;
            else
                transform.position = start;
        }

        private void FixedUpdate()
        {
            if (!_initialized || _consumed)
                return;

            _elapsed += Time.fixedDeltaTime;
            var t = Mathf.Clamp01(_elapsed / _duration);
            var position = EvaluateProjectilePosition(_start, _control, _end, t);

            if (_body != null)
                _body.MovePosition(position);
            else
                transform.position = position;

            TryProximityCatch(position);

            if (t >= 1f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other) => TryCatch(other);

        private void OnTriggerStay2D(Collider2D other) => TryCatch(other);

        private void TryProximityCatch(Vector2 position)
        {
            if (_goalkeeper == null)
                return;

            var keeperPosition = (Vector2)_goalkeeper.transform.position;
            if (Vector2.Distance(position, keeperPosition) <= _catchRadius)
                Consume();
        }

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
            _statusEffects?.Apply(_definition);
            Destroy(gameObject);
        }

        private static Vector2 EvaluateProjectilePosition(Vector2 start, Vector2 control, Vector2 end, float t)
        {
            var u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }
    }
}
