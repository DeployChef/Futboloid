using UnityEngine;

namespace Futboloid.Gameplay.Ball
{
    /// <summary>
    /// Точка привязки мяча: кик-офф, ведение у игрока/врага.
    /// </summary>
    public interface IBallAnchor
    {
        Vector2 WorldPosition { get; }
    }
}
