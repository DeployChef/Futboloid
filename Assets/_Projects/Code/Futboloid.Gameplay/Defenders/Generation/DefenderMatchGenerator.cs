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
            var pacing = settings.EvaluatePacing(matchNumber);
            var hpMul = context.EnemyHpMultiplier;

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
                DamageCooldown = 0.4f,
                PatrolPointCount = 4,
                PatrolRadius = 1.5f,
                WanderRadius = 1.5f,
                ChaseRadius = 3f,
                SeparationRadius = 0.6f,
                FieldMoveSpeed = 1.6f,
                FieldAcceleration = 12f,
                FieldArriveThreshold = 0.12f
            };

            if (matchNumber <= 1)
            {
                GenerateTutorial(settings, result, pacing, hpMul);
                return result;
            }

            var tier = settings.ResolveTier(matchNumber);
            var rng = CreateRng(settings.GenerationSeedSalt, matchNumber);
            var formation = PickFormation(settings, tier.Tier, rng);
            if (formation.SlotIds == null || formation.SlotIds.Length == 0)
                return result;

            var slotCount = Mathf.Min(formation.SlotIds.Length, tier.MaxFieldCount);
            for (var i = 0; i < slotCount; i++)
            {
                var slotId = formation.SlotIds[i];
                if (!IsValidSlot(settings, slotId))
                    continue;

                var archetype = PickArchetype(settings, tier.Tier, rng);
                if (string.IsNullOrEmpty(archetype.Id))
                    continue;

                var hp = settings.ResolveFieldHp(
                    archetype.Hp > 0 ? archetype.Hp : settings.FieldBaseHp,
                    pacing,
                    hpMul);

                result.Field.Add(archetype.ToBuild(slotId, hp));
            }

            return result;
        }

        private static void GenerateTutorial(
            DefenderGenerationSettings settings,
            DefenderGenerationResult result,
            float pacing,
            float hpMul)
        {
            var tutorial = settings.TutorialMatch;
            var slots = tutorial.SlotIds;
            var archetypeIds = tutorial.ArchetypeIds;
            if (slots == null || slots.Length == 0)
                return;

            for (var i = 0; i < slots.Length; i++)
            {
                var slotId = slots[i];
                if (!IsValidSlot(settings, slotId))
                    continue;

                var archetypeId = archetypeIds != null && i < archetypeIds.Length
                    ? archetypeIds[i]
                    : "Shield";

                if (!TryFindArchetype(settings, archetypeId, out var archetype))
                    continue;

                var hp = settings.ResolveFieldHp(
                    archetype.Hp > 0 ? archetype.Hp : settings.FieldBaseHp,
                    pacing,
                    hpMul);

                result.Field.Add(archetype.ToBuild(slotId, hp));
            }
        }

        private static FormationShapeDefinition PickFormation(
            DefenderGenerationSettings settings,
            int tier,
            System.Random rng)
        {
            var candidates = new List<FormationShapeDefinition>();
            var totalWeight = 0f;

            var formations = settings.Formations;
            if (formations == null)
                return default;

            for (var i = 0; i < formations.Length; i++)
            {
                var formation = formations[i];
                if (formation.MinTier > tier || formation.SlotIds == null || formation.SlotIds.Length == 0)
                    continue;

                if (formation.Weight <= 0f)
                    continue;

                candidates.Add(formation);
                totalWeight += formation.Weight;
            }

            if (candidates.Count == 0)
            {
                for (var i = 0; i < formations.Length; i++)
                {
                    if (formations[i].SlotIds != null && formations[i].SlotIds.Length > 0)
                        return formations[i];
                }

                return default;
            }

            var roll = (float)rng.NextDouble() * totalWeight;
            for (var i = 0; i < candidates.Count; i++)
            {
                roll -= candidates[i].Weight;
                if (roll <= 0f)
                    return candidates[i];
            }

            return candidates[candidates.Count - 1];
        }

        private static DefenderArchetypeDefinition PickArchetype(
            DefenderGenerationSettings settings,
            int tier,
            System.Random rng)
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

                totalWeight += archetype.Weight;
            }

            if (totalWeight <= 0f)
            {
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

                roll -= archetype.Weight;
                if (roll <= 0f)
                    return archetype;
            }

            return archetypes[archetypes.Length - 1];
        }

        private static bool TryFindArchetype(
            DefenderGenerationSettings settings,
            string id,
            out DefenderArchetypeDefinition archetype)
        {
            archetype = default;
            if (string.IsNullOrEmpty(id))
                return false;

            var archetypes = settings.Archetypes;
            if (archetypes == null)
                return false;

            for (var i = 0; i < archetypes.Length; i++)
            {
                if (archetypes[i].Id != id)
                    continue;

                archetype = archetypes[i];
                return true;
            }

            return false;
        }

        private static bool IsValidSlot(DefenderGenerationSettings settings, int slotId) =>
            slotId >= 0 && slotId < settings.SlotCount;

        private static System.Random CreateRng(int salt, int matchNumber)
        {
            unchecked
            {
                var seed = salt * 397 ^ matchNumber * 1009;
                return new System.Random(seed);
            }
        }
    }
}
