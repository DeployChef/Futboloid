using System;
using System.Collections.Generic;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Выбор фигуры, архетипов и статов на матч.</summary>
    public static class DefenderMatchGenerator
    {
        public static DefenderGenerationResult Generate(
            DefenderGenerationSettings settings,
            in DefenderGenerationContext context)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var result = new DefenderGenerationResult();
            var matchNumber = context.MatchNumber;
            var totalMatches = context.TotalMatches;
            var pacing = settings.EvaluatePacing(matchNumber, totalMatches);
            var hpMul = context.EnemyHpMultiplier;
            var fieldDefaults = settings.FieldDefaults;
            var rng = CreateRng(settings.GenerationSeedSalt, matchNumber, context.RunSeed);

            result.Goalkeeper = new DefenderBuild
            {
                SlotId = DefenderGenerationSettings.GoalkeeperSlotId,
                Role = DefenderRole.Goalkeeper,
                MaxHp = settings.ResolveGoalkeeperHp(matchNumber, hpMul),
                HitType = DefenderHitType.Reflect,
                MovementType = DefenderMovementType.Idle,
                TrackSpeed = settings.GkTrackSpeed,
                LaunchSpeed = 12f,
                OpenGoalChancePercent = 70,
                InteractionCooldown = fieldDefaults.InteractionCooldown,
                PatrolPointCount = Mathf.Max(1, fieldDefaults.PatrolPointCount),
                PatrolRadius = fieldDefaults.PatrolRadius,
                WanderRadius = fieldDefaults.PatrolRadius,
                SeparationRadius = fieldDefaults.SeparationRadius,
                FieldMoveSpeed = 1.6f,
                FieldAcceleration = fieldDefaults.FieldAcceleration,
                FieldArriveThreshold = fieldDefaults.FieldArriveThreshold,
                PointValue = settings.GkPointValue
            };

            GenerateFieldDefenders(settings, result, matchNumber, totalMatches, pacing, hpMul, fieldDefaults, rng);
            return result;
        }

        private static void GenerateFieldDefenders(
            DefenderGenerationSettings settings,
            DefenderGenerationResult result,
            int matchNumber,
            int totalMatches,
            float pacing,
            float hpMul,
            in FieldBehaviorDefaults fieldDefaults,
            System.Random rng)
        {
            var tier = settings.ResolveTier(matchNumber, totalMatches);
            var fieldCount = settings.ResolveFieldCount(matchNumber, totalMatches, rng);
            var zone = settings.ResolvePlacementZone(matchNumber, totalMatches);
            var slotIds = DefenderFormationComposer.Compose(settings, tier.Tier, fieldCount, zone, rng);
            if (slotIds.Count == 0)
                return;

            var pickedArchetypeIds = new List<string>(slotIds.Count);

            for (var i = 0; i < slotIds.Count; i++)
            {
                var slotId = slotIds[i];
                if (!IsValidSlot(settings, slotId))
                    continue;

                var archetype = PickArchetype(settings, tier.Tier, rng, pickedArchetypeIds);
                if (string.IsNullOrEmpty(archetype.Id))
                    continue;

                pickedArchetypeIds.Add(archetype.Id);

                var hp = settings.ResolveFieldHp(
                    archetype.Hp > 0 ? archetype.Hp : settings.FieldBaseHp,
                    pacing,
                    hpMul);

                result.Field.Add(archetype.ToBuild(slotId, hp, fieldDefaults));
            }
        }

        private static DefenderArchetypeDefinition PickArchetype(
            DefenderGenerationSettings settings,
            int tier,
            System.Random rng,
            IReadOnlyList<string> alreadyPicked)
        {
            var archetypes = settings.Archetypes;
            if (archetypes == null || archetypes.Length == 0)
                return default;

            var totalWeight = 0f;
            for (var i = 0; i < archetypes.Length; i++)
            {
                var archetype = archetypes[i];
                if (archetype.MinTier > tier || archetype.Weight <= 0f)
                    continue;

                if (IsArchetypeCapped(archetype.Id, alreadyPicked))
                    continue;

                totalWeight += archetype.Weight;
            }

            if (totalWeight <= 0f)
            {
                for (var i = 0; i < archetypes.Length; i++)
                {
                    if (archetypes[i].MinTier <= tier && !IsArchetypeCapped(archetypes[i].Id, alreadyPicked))
                        return archetypes[i];
                }

                for (var i = 0; i < archetypes.Length; i++)
                {
                    if (archetypes[i].MinTier <= tier)
                        return archetypes[i];
                }

                return archetypes[0];
            }

            var roll = (float)rng.NextDouble() * totalWeight;
            for (var i = 0; i < archetypes.Length; i++)
            {
                var archetype = archetypes[i];
                if (archetype.MinTier > tier || archetype.Weight <= 0f)
                    continue;

                if (IsArchetypeCapped(archetype.Id, alreadyPicked))
                    continue;

                roll -= archetype.Weight;
                if (roll <= 0f)
                    return archetype;
            }

            return archetypes[archetypes.Length - 1];
        }

        private static bool IsArchetypeCapped(string archetypeId, IReadOnlyList<string> alreadyPicked)
        {
            if (string.IsNullOrEmpty(archetypeId) || alreadyPicked == null)
                return false;

            var count = 0;
            for (var i = 0; i < alreadyPicked.Count; i++)
            {
                if (alreadyPicked[i] == archetypeId)
                    count++;
            }

            return count >= DefenderGenerationSettings.MaxSameArchetypePerMatch;
        }

        private static bool IsValidSlot(DefenderGenerationSettings settings, int slotId) =>
            slotId >= 0 && slotId < settings.SlotCount;

        private static System.Random CreateRng(int salt, int matchNumber, int runSeed)
        {
            unchecked
            {
                var seed = salt * 397 ^ matchNumber * 1009 ^ runSeed * 9176;
                return new System.Random(seed);
            }
        }
    }
}
