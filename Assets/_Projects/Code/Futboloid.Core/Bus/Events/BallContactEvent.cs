using Futboloid.Core;
using Futboloid.Core.Bus.Events;
using UnityEngine;

namespace Futboloid.Core.Bus.Events
{
    public readonly struct BallContactEvent
    {
        public BallContactKind Kind { get; }
        public int SlotId { get; }
        public Vector2 Point { get; }
        public Vector2 Normal { get; }
        public float Speed { get; }

        public BallContactEvent(
            BallContactKind kind,
            Vector2 point,
            Vector2 normal,
            float speed,
            int slotId = -1)
        {
            Kind = kind;
            Point = point;
            Normal = normal;
            Speed = speed;
            SlotId = slotId;
        }
    }
}
