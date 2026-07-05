using UnityEngine;

public class BallRollingUV : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float ballRadius = 0.5f;
    [SerializeField] private float rollSpeed = 1f;
    
    private Material _material;
    private Vector2 _lastPosition;
    private float _totalAngle;

    void Start()
    {
        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            _material = spriteRenderer.material;
            _lastPosition = new Vector2(transform.position.x, transform.position.y);
        }
    }

    void Update()
    {
        if (_material == null) return;

        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 delta = currentPosition - _lastPosition;
        
        if (delta.sqrMagnitude > 0.0001f)
        {
            float distance = delta.magnitude;
            float angleDelta = (distance / ballRadius) * Mathf.Rad2Deg * rollSpeed;
            
            // ✅ Определение направления вращения для любого направления движения
            float direction = -Mathf.Sign(delta.y);
            if (Mathf.Abs(delta.y) < 0.001f)
                direction = -Mathf.Sign(delta.x);
            
            angleDelta *= direction;
            
            _totalAngle += angleDelta;
            _material.SetFloat("_RotationAngle", _totalAngle);
        }

        _lastPosition = currentPosition;
    }
}