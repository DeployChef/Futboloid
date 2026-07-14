using UnityEngine;

namespace Futboloid.UI.Shaders
{
    /// <summary>
    ///     Сдвигает UV текстуры против движения мяча — иллюзия качения.
    ///     Требует в шейдере свойство <c>_UVOffset</c> (Vector2) и Repeat на текстуре.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BallRollingUV : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private float ballRadius = 0.5f;

        [SerializeField]
        [Tooltip("1.0 = один полный повтор текстуры на длину окружности (2πR).")]
        private float rollSpeed = 1f;

        [SerializeField]
        [Tooltip("Множитель знака по осям, если вертикаль/горизонталь едут не туда. По умолчанию (-1, -1).")]
        private Vector2 rollSign = new(-1f, -1f);

        private Material _material;
        private Vector2 _lastPosition;
        private Vector2 _uvOffset;

        private static readonly int UvOffsetId = Shader.PropertyToID("_UVOffset");

        private void Start()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"[{nameof(BallRollingUV)}] SpriteRenderer not found on {gameObject.name}", this);
                enabled = false;
                return;
            }

            _material = spriteRenderer.material;
            _lastPosition = transform.position;
            _material.SetVector(UvOffsetId, _uvOffset);
        }

        private void Update()
        {
            if (_material == null)
                return;

            Vector2 currentPosition = transform.position;
            Vector2 delta = currentPosition - _lastPosition;

            if (delta.sqrMagnitude > 0.0001f)
            {
                float circumference = 2f * Mathf.PI * ballRadius;
                Vector2 scroll = delta / circumference * rollSpeed;
                scroll.Scale(rollSign);

                _uvOffset += scroll;
                _material.SetVector(UvOffsetId, _uvOffset);
            }

            _lastPosition = currentPosition;
        }

        /// <summary>Сбросить сдвиг (например после телепорта мяча).</summary>
        public void ResetOffset()
        {
            _uvOffset = Vector2.zero;
            if (_material != null)
                _material.SetVector(UvOffsetId, _uvOffset);
        }
    }
}
