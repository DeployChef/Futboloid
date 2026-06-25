using UnityEngine;

public class EnemyGoalkeeper : MonoBehaviour
{
    public Transform ball;
    public float speed = 4f;
    public float minX = -3f;
    public float maxX = 3f;
    public float arcHeight = 2f;
    public int arcSegments = 20;

    private float centerY;
    private float currentX;

    void Start()
    {
        centerY = transform.position.y;
        currentX = transform.position.x;
    }

    void Update()
    {
        if (ball == null) return;

        float targetX = Mathf.Clamp(ball.position.x, minX, maxX);
        currentX = Mathf.MoveTowards(currentX, targetX, speed * Time.deltaTime);

        // Гипербола: Y выше к краям, ниже в центре
        float halfRange = (maxX - minX) / 2f;
        float xOffset = currentX - ((minX + maxX) / 2f);
        float t = xOffset / halfRange; // -1 до 1
        float y = centerY + arcHeight * (t * t * t * t); // t^4 для плавной гиперболы

        transform.position = new Vector2(currentX, y);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) centerY = transform.position.y;

        Gizmos.color = Color.yellow;
        Vector2 prevPoint = Vector2.zero;
        float halfRange = (maxX - minX) / 2f;

        for (int i = 0; i <= arcSegments; i++)
        {
            float t = -1f + 2f * i / arcSegments;
            float x = Mathf.Lerp(minX, maxX, (t + 1f) / 2f);
            float y = centerY + arcHeight * (t * t * t * t);
            Vector2 point = new Vector2(x, y);

            if (i > 0)
                Gizmos.DrawLine(prevPoint, point);

            prevPoint = point;
        }
    }
}