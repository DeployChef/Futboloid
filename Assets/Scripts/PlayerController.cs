using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float preLaunchMinX = -4;
    [SerializeField] private float preLaunchMaxX = 4f;
    [SerializeField] private float gameMaxX = 4f;
    [SerializeField] private float gameMinX = 4f;

    private PlayerInput _playerInput;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        var move = _playerInput.actions["Move"].ReadValue<Vector2>();
        transform.Translate(Vector2.right * move.x * speed * Time.deltaTime);

        float minX, maxX;

        //if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Waiting)
        //{
        //    minX = preLaunchMinX;
        //    maxX = preLaunchMaxX;
        //}
        //else
        //{
            minX = gameMinX;
            maxX = gameMaxX;
        //}

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector2(clampedX, transform.position.y);
    }
}
