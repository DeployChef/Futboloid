using System.Collections.Generic;
using Futboloid.Gameplay.Ball;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    public interface IDefenderBallContact
    {
        int SlotId { get; }
        bool IsAlive { get; }

        bool TryHandleBallHit(BallMotion motion, RaycastHit2D hit, HashSet<int> hitsThisFrame);
    }
}
