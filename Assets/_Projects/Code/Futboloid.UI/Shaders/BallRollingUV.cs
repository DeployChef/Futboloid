using UnityEngine;

namespace Futboloid.UI.Shaders
{
    /// <summary>
    ///     Computes the rolling UV rotation angle for a 2D ball based on its movement.
    ///     Updates the material's "_RotationAngle" property used by BallShader.shadergraph.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BallRollingUV : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private float ballRadius = 0.5f;

        [SerializeField]
        [Tooltip("Множитель скорости вращения относительно пройденного расстояния. " +
                 "1.0 = физически корректное вращение (длина окружности = 2πR).")]
        private float rollSpeed = 1f;

        private Material _material;
        private Vector2 _lastPosition;
        private float _totalAngle;

        private static readonly int RotationAngle = Shader.PropertyToID("_RotationAngle");

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
            _lastPosition = new Vector2(transform.position.x, transform.position.y);
        }

        private void Update()
        {
            if (_material == null)
                return;

            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
            Vector2 delta = currentPosition - _lastPosition;

            if (delta.sqrMagnitude > 0.0001f)
            {
                float distance = delta.magnitude;

                // Угол поворота на основе пройденного пути: angle = distance / radius (радианы)
                float angleDelta = (distance / ballRadius) * Mathf.Rad2Deg * rollSpeed;

                // Определение направления вращения через векторное произведение (cross product):
                // Для 2D: cross(delta, up) = delta.x * 0 - delta.y * 0 ... 
                // Используем псевдо-кросс: direction = sign(delta.x * forwardZ - delta.y * forwardX)
                // В 2D плоскости (X-right, Y-up) вращение против часовой = положительное.
                // При движении вправо (delta.x > 0) мяч должен крутиться по часовой => отрицательное направление.
                float direction = -Mathf.Sign(delta.x);

                angleDelta *= direction;
                _totalAngle += angleDelta;

                // Предотвращаем потерю точности float при бесконечном суммировании
                _totalAngle = Mathf.Repeat(_totalAngle, 360f);

                _material.SetFloat(RotationAngle, _totalAngle);
            }

            _lastPosition = currentPosition;
        }
    }
}