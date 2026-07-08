using System;
using System.Collections.Generic;

namespace Futboloid.Gameplay.Defenders
{
    public static class DefenderBehaviorMapping
    {
        public static DefenderBehaviorKind From(DefenderHitType hit, DefenderMovementType move) =>
            (hit, move) switch
            {
                (DefenderHitType.Reflect, DefenderMovementType.Idle) => DefenderBehaviorKind.ReflectIdle,
                (DefenderHitType.Reflect, DefenderMovementType.WanderInRadius) => DefenderBehaviorKind.ReflectWander,
                (DefenderHitType.Reflect, DefenderMovementType.ChaseBall) => DefenderBehaviorKind.ReflectChase,
                (DefenderHitType.Reflect, DefenderMovementType.PatrolGenerated) => DefenderBehaviorKind.ReflectPatrol,
                (DefenderHitType.ToPlayerGoal, DefenderMovementType.Idle) => DefenderBehaviorKind.ShootIdle,
                (DefenderHitType.ToPlayerGoal, DefenderMovementType.WanderInRadius) => DefenderBehaviorKind.ShootWander,
                (DefenderHitType.ToPlayerGoal, DefenderMovementType.ChaseBall) => DefenderBehaviorKind.ShootChase,
                (DefenderHitType.ToPlayerGoal, DefenderMovementType.PatrolGenerated) => DefenderBehaviorKind.ShootPatrol,
                _ => DefenderBehaviorKind.ReflectIdle
            };

        public static (DefenderHitType Hit, DefenderMovementType Move) ToTypes(DefenderBehaviorKind kind) =>
            kind switch
            {
                DefenderBehaviorKind.ReflectIdle => (DefenderHitType.Reflect, DefenderMovementType.Idle),
                DefenderBehaviorKind.ReflectWander => (DefenderHitType.Reflect, DefenderMovementType.WanderInRadius),
                DefenderBehaviorKind.ReflectChase => (DefenderHitType.Reflect, DefenderMovementType.ChaseBall),
                DefenderBehaviorKind.ReflectPatrol => (DefenderHitType.Reflect, DefenderMovementType.PatrolGenerated),
                DefenderBehaviorKind.ShootIdle => (DefenderHitType.ToPlayerGoal, DefenderMovementType.Idle),
                DefenderBehaviorKind.ShootWander => (DefenderHitType.ToPlayerGoal, DefenderMovementType.WanderInRadius),
                DefenderBehaviorKind.ShootChase => (DefenderHitType.ToPlayerGoal, DefenderMovementType.ChaseBall),
                DefenderBehaviorKind.ShootPatrol => (DefenderHitType.ToPlayerGoal, DefenderMovementType.PatrolGenerated),
                _ => (DefenderHitType.Reflect, DefenderMovementType.Idle)
            };

        public static bool TryGetKindForArchetype(string archetypeId, out DefenderBehaviorKind kind)
        {
            kind = archetypeId switch
            {
                "Shield" => DefenderBehaviorKind.ReflectIdle,
                "Tank" => DefenderBehaviorKind.ReflectIdle,
                "Drifter" => DefenderBehaviorKind.ReflectWander,
                "Hunter" => DefenderBehaviorKind.ReflectChase,
                "Sniper" => DefenderBehaviorKind.ShootIdle,
                "Striker" => DefenderBehaviorKind.ShootChase,
                "Presser" => DefenderBehaviorKind.ShootChase,
                _ => default
            };

            return archetypeId is "Shield" or "Tank" or "Drifter" or "Hunter" or "Sniper" or "Striker" or "Presser";
        }

        public static IReadOnlyList<string> GetArchetypeIds(DefenderBehaviorKind kind) =>
            kind switch
            {
                DefenderBehaviorKind.ReflectIdle => ArchetypeIds.ShieldTank,
                DefenderBehaviorKind.ReflectWander => ArchetypeIds.Drifter,
                DefenderBehaviorKind.ReflectChase => ArchetypeIds.Hunter,
                DefenderBehaviorKind.ShootIdle => ArchetypeIds.Sniper,
                DefenderBehaviorKind.ShootChase => ArchetypeIds.StrikerPresser,
                _ => Array.Empty<string>()
            };

        public static string GetShortLabel(DefenderBehaviorKind kind) =>
            kind switch
            {
                DefenderBehaviorKind.ReflectIdle => "Wall",
                DefenderBehaviorKind.ReflectWander => "Drift",
                DefenderBehaviorKind.ReflectChase => "Hunt",
                DefenderBehaviorKind.ReflectPatrol => "Patrol-R",
                DefenderBehaviorKind.ShootIdle => "Snipe",
                DefenderBehaviorKind.ShootWander => "Drift-G",
                DefenderBehaviorKind.ShootChase => "Press",
                DefenderBehaviorKind.ShootPatrol => "Patrol-G",
                _ => kind.ToString()
            };

        private static class ArchetypeIds
        {
            public static readonly string[] ShieldTank = { "Shield", "Tank" };
            public static readonly string[] Drifter = { "Drifter" };
            public static readonly string[] Hunter = { "Hunter" };
            public static readonly string[] Sniper = { "Sniper" };
            public static readonly string[] StrikerPresser = { "Striker", "Presser" };
        }
    }
}
