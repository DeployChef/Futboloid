using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Vector2 _direction = Vector2.up;
    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0;
        _rb.linearDamping = 0;
        _rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void Start()
    {
        var angle = Random.Range(-30f, 30f);
        _direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Playing)
        {
            _rb.linearVelocity = _direction.normalized * speed;
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }



    private void OnCollisionEnter2D(Collision2D other)
    {
        var normal = other.contacts[0].normal;
        _direction = Vector2.Reflect(_direction, normal);
    }

    // Проверка зоны смерти (триггеры)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("GameOver"))
        {
            GameManager.Instance.EndMatch();
        }
    }
}
