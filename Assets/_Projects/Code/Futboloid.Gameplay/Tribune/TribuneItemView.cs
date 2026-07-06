using Futboloid.Core.StatusEffects;
using Futboloid.Gameplay.Keeper;
using UnityEngine;

namespace Futboloid.Gameplay.Tribune
{
    /// <summary>
    /// Предмет с трибуны: полёт по дуге, при касании вратаря — timed-эффект.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TribuneItemView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private StatusEffectDefinition _definition;
        private IStatusEffectService _statusEffects;
        private Vector2 _start;
        private Vector2 _end;
        private float _arcHeight;
        private float _duration;
        private float _elapsed;
        private bool _consumed;
        private bool _initialized;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            var body = GetComponent<Rigidbody2D>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody2D>();

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

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
            _arcHeight = arcHeight;
            _duration = Mathf.Max(0.1f, durationSeconds);
            _elapsed = 0f;
            _consumed = false;
            _initialized = true;

            transform.position = start;

            if (spriteRenderer != null)
            {
                if (definition?.Icon != null)
                    spriteRenderer.sprite = definition.Icon;

                spriteRenderer.sortingOrder = 50;
            }
        }

        private void Update()
        {
            if (!_initialized || _consumed)
                return;

            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / _duration);
            transform.position = EvaluateArcPosition(_start, _end, _arcHeight, t);

            if (t >= 1f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized || _consumed)
                return;

            if (other.GetComponentInParent<GoalkeeperView>() == null)
                return;

            _consumed = true;
            _statusEffects?.Apply(_definition);
            Destroy(gameObject);
        }

        private static Vector3 EvaluateArcPosition(Vector2 start, Vector2 end, float arcHeight, float t)
        {
            var basePosition = Vector2.Lerp(start, end, t);
            var arc = Mathf.Sin(Mathf.PI * t) * arcHeight;
            return new Vector3(basePosition.x, basePosition.y + arc, 0f);
        }
    }
}
