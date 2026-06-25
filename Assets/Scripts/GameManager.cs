using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public enum GameState { Waiting, Playing, GameOver }
    public GameState currentState = GameState.Waiting;

    public Ball ball;
    public Transform arrowPivot;
    public Transform player;
    public float arrowAngle = 70f;

    private PlayerInput playerInput;

    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        ball.speed = 0;
        ball.transform.position = (Vector2)player.position + Vector2.up * 1.5f;
        ball.direction = Vector2.up;
    }

    void Update()
    {
        if (ball == null) return;

        switch (currentState)
        {
            case GameState.Waiting:
                UpdateArrow();
                break;

            case GameState.Playing:
                if (arrowPivot.gameObject.activeSelf)
                    arrowPivot.gameObject.SetActive(false);
                break;
        }
    }

    void UpdateArrow()
    {
        if (arrowPivot == null || player == null || ball == null) return;

        // Стрелка над мячом
        arrowPivot.position = ball.transform.position;

        // Направление от игрока к мячу (куда полетит)
        Vector2 toBall = (Vector2)ball.transform.position - (Vector2)player.position;
        float angle = Mathf.Atan2(toBall.x, toBall.y) * Mathf.Rad2Deg;

        // Ограничиваем угол
        angle = Mathf.Clamp(angle, -arrowAngle, arrowAngle);

        arrowPivot.rotation = Quaternion.Euler(0, 0, -angle);
    }

    public void OnLaunch(InputValue value)
    {
        if (currentState == GameState.Waiting)
            LaunchBall();
    }

    void LaunchBall()
    {
        currentState = GameState.Playing;

        Vector2 dir = arrowPivot.up;
        ball.direction = dir;
        ball.speed = 7f;
    }
}