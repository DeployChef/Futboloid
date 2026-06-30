using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Keeper;
using Futboloid.Gameplay.Physics;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    public static class DefenderHitResolver
    {
        public static void Resolve(
            BallMotion motion,
            RaycastHit2D hit,
            DefenderView self,
            DefenderHitBehavior behavior)
        {
            if (motion == null)
                return;

            if (behavior == null)
            {
                motion.ReflectFromHit(hit);
                return;
            }

            switch (behavior.HitType)
            {
                case DefenderHitType.Reflect:
                    motion.ReflectFromHit(hit);
                    break;

                case DefenderHitType.ToPlayerGoal:
                    LaunchToPlayerGoal(motion, hit, behavior, self);
                    break;

                default:
                    motion.ReflectFromHit(hit);
                    break;
            }
        }

        private static void LaunchToPlayerGoal(
            BallMotion motion,
            RaycastHit2D hit,
            DefenderHitBehavior behavior,
            DefenderView self)
        {
            var origin = HitOrigin(hit, self);
            var aimOpen = Random.Range(0, 100) < behavior.OpenGoalChancePercent;
            var target = aimOpen ? ResolveOpenGoalPoint() : ResolveKeeperPoint();
            var delta = target - origin;

            if (delta.sqrMagnitude < 0.0001f)
            {
                motion.LaunchDirected(Vector2.down, behavior.LaunchSpeed);
                return;
            }

            motion.LaunchDirected(delta, behavior.LaunchSpeed);
        }

        private static Vector2 HitOrigin(RaycastHit2D hit, DefenderView self)
        {
            if (hit.collider != null)
                return hit.point;

            return self != null ? (Vector2)self.transform.position : Vector2.zero;
        }

        private static Vector2 ResolveOpenGoalPoint()
        {
            var anchor = Object.FindAnyObjectByType<PlayerGoalAnchor>();
            if (anchor != null)
                return anchor.AimPoint;

            var colliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider != null && collider.gameObject.layer == PhysicsLayers.GoalPlayerId)
                    return collider.bounds.center;
            }

            return new Vector2(0f, -4f);
        }

        private static Vector2 ResolveKeeperPoint()
        {
            var keeper = Object.FindAnyObjectByType<GoalkeeperView>();
            return keeper != null ? keeper.transform.position : ResolveOpenGoalPoint();
        }
    }
}
