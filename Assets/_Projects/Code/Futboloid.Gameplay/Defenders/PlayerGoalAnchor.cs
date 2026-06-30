using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Точка прицеливания в ворота игрока (центр триггера проёма).</summary>
    public class PlayerGoalAnchor : MonoBehaviour
    {
        public Vector2 AimPoint
        {
            get
            {
                var collider = GetComponent<Collider2D>();
                return collider != null ? collider.bounds.center : transform.position;
            }
        }
    }
}
