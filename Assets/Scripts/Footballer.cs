using UnityEngine;

public class Footballer : MonoBehaviour
{
    public enum MoveType { Standing, Random, ChaseBall, Trajectory }

    public MoveType moveType = MoveType.Standing;
    public float moveSpeed = 2f;
    public float minX = -4f;
    public float maxX = 4f;
    public float minY = -5f;
    public float maxY = 5f;

    // Random
    private Vector2 randomTarget;
    private float changeTargetTimer = 0f;
    public float changeTargetInterval = 2f;

    // ChaseBall
    public Transform ball;

    // Trajectory
    public Transform[] waypoints;
    private int currentWaypoint = 0;

    void Start()
    {
        SetNewRandomTarget();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Playing)
            return;

        switch (moveType)
        {
            case MoveType.Standing:
                break;
            case MoveType.Random:
                RandomMove();
                break;
            case MoveType.ChaseBall:
                ChaseBall();
                break;
            case MoveType.Trajectory:
                FollowTrajectory();
                break;
        }
    }

    void RandomMove()
    {
        changeTargetTimer -= Time.deltaTime;
        if (changeTargetTimer <= 0 || Vector2.Distance(transform.position, randomTarget) < 0.1f)
            SetNewRandomTarget();

        transform.position = Vector2.MoveTowards(transform.position, randomTarget, moveSpeed * Time.deltaTime);
    }

    void SetNewRandomTarget()
    {
        randomTarget = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        changeTargetTimer = changeTargetInterval;
    }

    void ChaseBall()
    {
        if (ball == null) return;
        Vector2 target = ball.position;
        target.x = Mathf.Clamp(target.x, minX, maxX);
        target.y = Mathf.Clamp(target.y, minY, maxY);
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    void FollowTrajectory()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }
}