using Assets.Scripts;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector2 direction = Vector2.up;
    public float speed = 5f;
    public float minSpeed = 2f;
    public float slowDownRate = 0.1f;
    public float boostSpeed = 7f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void FixedUpdate()
    {
        if (speed <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = direction.normalized * speed;
    }
    void Update()
    {
        if (speed <= 0) return;

        if (speed > minSpeed)
            speed -= slowDownRate * Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"БАМ! Столкновение с: {collision.gameObject.name}");
        Vector2 normal = collision.contacts[0].normal;
        direction = Vector2.Reflect(direction, normal);

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            speed = boostSpeed;
        }

        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(1);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeadZone"))
        {
            Destroy(gameObject);
        }
    }
}