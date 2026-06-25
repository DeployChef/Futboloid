using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 8f;
    public float preLaunchMinX = -2f;
    public float preLaunchMaxX = 2f;
    public float gameMinX = -4f;
    public float gameMaxX = 4f;

    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        Vector2 move = playerInput.actions["Move"].ReadValue<Vector2>();
        transform.Translate(Vector2.right * move.x * speed * Time.deltaTime);

        float minX, maxX;

        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Waiting)
        {
            minX = preLaunchMinX;
            maxX = preLaunchMaxX;
        }
        else
        {
            minX = gameMinX;
            maxX = gameMaxX;
        }

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector2(clampedX, transform.position.y);
    }
}