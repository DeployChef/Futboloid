using System;
using UnityEngine;

public class EnemyGoalkeeper : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private float speed = 4f;
    [SerializeField] private float minX = -3f;
    [SerializeField] private float maxX = 3f;

    private float _currentX;
    private float _currentY;

    private void Start()
    {
        _currentY = transform.position.y;
    }

    void Update()
    {
        if(!ball) return;

        var targetX = Mathf.Clamp(ball.position.x, minX, maxX);
        _currentX = Mathf.MoveTowards(_currentX, targetX, speed * Time.deltaTime);

        var halfRange = (maxX - minX) / 2f;
        var xOffset = _currentX - ((minX + maxX) / 2f);
        var t = xOffset / halfRange;
        var y = _currentY + 2f * (t * t * t * t);

        transform.position = new Vector2(_currentX, y);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) _currentY = transform.position.y;

        Gizmos.color = Color.yellow;
        var prevPoint = Vector2.zero;
        var halfRange = (maxX - minX) / 2f;

        for (int i = 0; i < 20; i++)
        {
            var t = -1f + 2f * i / 20;
            var x = Mathf.Lerp(minX, maxX, (t + 1f) / 2f);
            var y = _currentY + 2f * (t * t * t * t);
            var point = new Vector2(x, y);

            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }

            prevPoint = point;
        }
    }
}
