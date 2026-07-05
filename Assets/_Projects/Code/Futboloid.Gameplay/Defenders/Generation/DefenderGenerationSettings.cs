using System;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    [CreateAssetMenu(fileName = "DefenderGenerationSettings", menuName = "Futboloid/Defender Generation Settings")]
    public sealed class DefenderGenerationSettings : ScriptableObject
    {
        public const int GoalkeeperSlotId = 100;
        public const string ResourcePath = "Data/Settings/DefenderGenerationSettings";

        [SerializeField] private int gridColumns = 5;
        [SerializeField] private int gridRows = 7;
        [SerializeField] private int generationSeedSalt = 7919;

        [Header("Goalkeeper")]
        [SerializeField] private int gkBaseHp = 10;
        [SerializeField] private int gkHpPerMatch = 2;
        [SerializeField] private float gkTrackSpeed = 1.25f;

        [Header("Field HP")]
        [SerializeField] private int fieldBaseHp = 3;
        [SerializeField] private AnimationCurve pacingCurve = AnimationCurve.Linear(1f, 0.35f, 10f, 1f);

        [Header("Scoring")]
        [SerializeField] private int gkPointValue = 30;

        [Header("Tutorial (match 1)")]
        [SerializeField] private TutorialMatchDefinition tutorialMatch = TutorialMatchDefinition.CreateDefault();

        [Header("Formations")]
        [SerializeField] private FormationShapeDefinition[] formations = Array.Empty<FormationShapeDefinition>();

        [Header("Archetypes")]
        [SerializeField] private DefenderArchetypeDefinition[] archetypes = Array.Empty<DefenderArchetypeDefinition>();

        [Header("Tiers")]
        [SerializeField] private MatchTierDefinition[] tiers = Array.Empty<MatchTierDefinition>();

        public int GridColumns => Mathf.Max(1, gridColumns);
        public int GridRows => Mathf.Max(1, gridRows);
        public int SlotCount => GridColumns * GridRows;
        public int GenerationSeedSalt => generationSeedSalt;
        public int GkBaseHp => Mathf.Max(1, gkBaseHp);
        public int GkHpPerMatch => Mathf.Max(0, gkHpPerMatch);
        public float GkTrackSpeed => Mathf.Max(0.1f, gkTrackSpeed);
        public int FieldBaseHp => Mathf.Max(1, fieldBaseHp);
        public int GkPointValue => Mathf.Max(1, gkPointValue);
        public AnimationCurve PacingCurve => pacingCurve;
        public TutorialMatchDefinition TutorialMatch => tutorialMatch;
        public FormationShapeDefinition[] Formations => formations;
        public DefenderArchetypeDefinition[] Archetypes => archetypes;
        public MatchTierDefinition[] Tiers => tiers;

        public static DefenderGenerationSettings Load()
        {
            var settings = Resources.Load<DefenderGenerationSettings>(ResourcePath);
            if (settings != null)
                return settings;

            Debug.LogWarning(
                $"[DefenderGenerationSettings] Asset not found at Resources/{ResourcePath}. " +
                "Using runtime defaults.");

            var runtime = CreateInstance<DefenderGenerationSettings>();
            runtime.EnsureDefaults();
            return runtime;
        }

        private void OnValidate() => EnsureDefaults();

        private void EnsureDefaults()
        {
            if (pacingCurve == null || pacingCurve.length == 0)
                pacingCurve = AnimationCurve.Linear(1f, 0.35f, 10f, 1f);

            if (tutorialMatch.SlotIds == null || tutorialMatch.SlotIds.Length == 0)
                tutorialMatch = TutorialMatchDefinition.CreateDefault();

            if (formations == null || formations.Length == 0)
                formations = FormationShapeDefinition.CreateDefaults();

            if (archetypes == null || archetypes.Length == 0)
                archetypes = DefenderArchetypeDefinition.CreateDefaults();

            if (tiers == null || tiers.Length == 0)
                tiers = MatchTierDefinition.CreateDefaults();
        }

        public float EvaluatePacing(int matchNumber)
        {
            if (pacingCurve == null || pacingCurve.length == 0)
                return 1f;

            return Mathf.Max(0.1f, pacingCurve.Evaluate(matchNumber));
        }

        public int ResolveGoalkeeperHp(int matchNumber, float hpMultiplier)
        {
            var fromMatch = GkBaseHp + (matchNumber - 1) * GkHpPerMatch;
            var scaled = fromMatch * hpMultiplier;
            return Mathf.Max(1, Mathf.RoundToInt(scaled));
        }

        public int ResolveFieldHp(int archetypeHp, float pacing, float hpMultiplier)
        {
            var scaled = archetypeHp * pacing * hpMultiplier;
            return Mathf.Max(1, Mathf.RoundToInt(scaled));
        }

        public MatchTierDefinition ResolveTier(int matchNumber)
        {
            var tiersList = Tiers;
            if (tiersList == null || tiersList.Length == 0)
                return MatchTierDefinition.CreateDefaults()[0];

            for (var i = tiersList.Length - 1; i >= 0; i--)
            {
                var tier = tiersList[i];
                if (matchNumber >= tier.MinMatchNumber)
                    return tier;
            }

            return tiersList[0];
        }
    }

    [Serializable]
    public struct TutorialMatchDefinition
    {
        public int[] SlotIds;
        public string[] ArchetypeIds;

        public static TutorialMatchDefinition CreateDefault() => new()
        {
            SlotIds = new[] { 12, 16, 18 },
            ArchetypeIds = new[] { "Shield", "Shield", "Drifter" }
        };
    }

    [Serializable]
    public struct FormationShapeDefinition
    {
        public string Id;
        public int[] SlotIds;
        public int MinTier;
        public float Weight;

        public static FormationShapeDefinition[] CreateDefaults() => new[]
        {
            new FormationShapeDefinition
            {
                Id = "Triangle3",
                SlotIds = new[] { 12, 16, 18 },
                MinTier = 0,
                Weight = 3f
            },
            new FormationShapeDefinition
            {
                Id = "Line3",
                SlotIds = new[] { 11, 12, 13 },
                MinTier = 1,
                Weight = 2f
            },
            new FormationShapeDefinition
            {
                Id = "V5",
                SlotIds = new[] { 10, 14, 17, 21, 23 },
                MinTier = 2,
                Weight = 2f
            },
            new FormationShapeDefinition
            {
                Id = "Wall5",
                SlotIds = new[] { 10, 11, 12, 13, 14 },
                MinTier = 2,
                Weight = 2.5f
            }
        };
    }

    [Serializable]
    public struct DefenderArchetypeDefinition
    {
        public string Id;
        public int MinTier;
        public float Weight;
        public int Hp;
        public DefenderHitType HitType;
        public DefenderMovementType MovementType;
        public int PatrolPointCount;
        public float PatrolRadius;
        public float WanderRadius;
        public float ChaseRadius;
        public float SeparationRadius;
        public float FieldMoveSpeed;
        public float FieldAcceleration;
        public float FieldArriveThreshold;
        public float LaunchSpeed;
        public int OpenGoalChancePercent;
        public float InteractionCooldown;
        public int PointValue;

        public DefenderBuild ToBuild(int slotId, int maxHp)
        {
            return new DefenderBuild
            {
                SlotId = slotId,
                Role = DefenderRole.Field,
                MaxHp = maxHp,
                HitType = HitType,
                MovementType = MovementType,
                PatrolPointCount = Mathf.Max(1, PatrolPointCount),
                PatrolRadius = PatrolRadius,
                WanderRadius = WanderRadius,
                ChaseRadius = ChaseRadius,
                SeparationRadius = SeparationRadius,
                FieldMoveSpeed = FieldMoveSpeed,
                FieldAcceleration = FieldAcceleration,
                FieldArriveThreshold = FieldArriveThreshold,
                LaunchSpeed = LaunchSpeed,
                OpenGoalChancePercent = OpenGoalChancePercent,
                InteractionCooldown = InteractionCooldown,
                TrackSpeed = 0f,
                PointValue = Mathf.Max(1, PointValue)
            };
        }

        public static DefenderArchetypeDefinition[] CreateDefaults() => new[]
        {
            new DefenderArchetypeDefinition
            {
                Id = "Shield",
                MinTier = 0,
                Weight = 4f,
                Hp = 3,
                PointValue = 10,
                HitType = DefenderHitType.Reflect,
                MovementType = DefenderMovementType.Idle,
                PatrolPointCount = 4,
                PatrolRadius = 1.2f,
                WanderRadius = 1.2f,
                ChaseRadius = 3f,
                SeparationRadius = 0.6f,
                FieldMoveSpeed = 1.4f,
                FieldAcceleration = 12f,
                FieldArriveThreshold = 0.12f,
                LaunchSpeed = 12f,
                OpenGoalChancePercent = 70,
                InteractionCooldown = 0.1f
            },
            new DefenderArchetypeDefinition
            {
                Id = "Drifter",
                MinTier = 0,
                Weight = 3f,
                Hp = 2,
                PointValue = 8,
                HitType = DefenderHitType.Reflect,
                MovementType = DefenderMovementType.WanderInRadius,
                PatrolPointCount = 4,
                PatrolRadius = 1.2f,
                WanderRadius = 1.6f,
                ChaseRadius = 3f,
                SeparationRadius = 0.6f,
                FieldMoveSpeed = 1.5f,
                FieldAcceleration = 12f,
                FieldArriveThreshold = 0.12f,
                LaunchSpeed = 12f,
                OpenGoalChancePercent = 70,
                InteractionCooldown = 0.1f
            },
            new DefenderArchetypeDefinition
            {
                Id = "Hunter",
                MinTier = 1,
                Weight = 2.5f,
                Hp = 3,
                PointValue = 12,
                HitType = DefenderHitType.Reflect,
                MovementType = DefenderMovementType.ChaseBallInRadius,
                PatrolPointCount = 4,
                PatrolRadius = 1.2f,
                WanderRadius = 1.2f,
                ChaseRadius = 3.5f,
                SeparationRadius = 0.6f,
                FieldMoveSpeed = 1.7f,
                FieldAcceleration = 14f,
                FieldArriveThreshold = 0.12f,
                LaunchSpeed = 12f,
                OpenGoalChancePercent = 70,
                InteractionCooldown = 0.1f
            },
            new DefenderArchetypeDefinition
            {
                Id = "Sniper",
                MinTier = 1,
                Weight = 1.5f,
                Hp = 2,
                PointValue = 15,
                HitType = DefenderHitType.ToPlayerGoal,
                MovementType = DefenderMovementType.Idle,
                PatrolPointCount = 4,
                PatrolRadius = 1f,
                WanderRadius = 1f,
                ChaseRadius = 2.5f,
                SeparationRadius = 0.6f,
                FieldMoveSpeed = 1.3f,
                FieldAcceleration = 12f,
                FieldArriveThreshold = 0.12f,
                LaunchSpeed = 13f,
                OpenGoalChancePercent = 85,
                InteractionCooldown = 0.1f
            },
            new DefenderArchetypeDefinition
            {
                Id = "Tank",
                MinTier = 2,
                Weight = 2f,
                Hp = 5,
                PointValue = 25,
                HitType = DefenderHitType.Reflect,
                MovementType = DefenderMovementType.Idle,
                PatrolPointCount = 4,
                PatrolRadius = 1f,
                WanderRadius = 1f,
                ChaseRadius = 2.5f,
                SeparationRadius = 0.7f,
                FieldMoveSpeed = 1.2f,
                FieldAcceleration = 10f,
                FieldArriveThreshold = 0.14f,
                LaunchSpeed = 11f,
                OpenGoalChancePercent = 60,
                InteractionCooldown = 0.1f
            },
            new DefenderArchetypeDefinition
            {
                Id = "Presser",
                MinTier = 3,
                Weight = 1.5f,
                Hp = 3,
                PointValue = 18,
                HitType = DefenderHitType.ToPlayerGoal,
                MovementType = DefenderMovementType.ChaseBallInRadius,
                PatrolPointCount = 4,
                PatrolRadius = 1f,
                WanderRadius = 1.2f,
                ChaseRadius = 4f,
                SeparationRadius = 0.6f,
                FieldMoveSpeed = 1.8f,
                FieldAcceleration = 14f,
                FieldArriveThreshold = 0.1f,
                LaunchSpeed = 13f,
                OpenGoalChancePercent = 75,
                InteractionCooldown = 0.1f
            }
        };
    }

    [Serializable]
    public struct MatchTierDefinition
    {
        public int MinMatchNumber;
        public int Tier;
        public int MaxFieldCount;

        public static MatchTierDefinition[] CreateDefaults() => new[]
        {
            new MatchTierDefinition { MinMatchNumber = 1, Tier = 0, MaxFieldCount = 3 },
            new MatchTierDefinition { MinMatchNumber = 2, Tier = 1, MaxFieldCount = 4 },
            new MatchTierDefinition { MinMatchNumber = 4, Tier = 2, MaxFieldCount = 5 },
            new MatchTierDefinition { MinMatchNumber = 7, Tier = 3, MaxFieldCount = 7 }
        };
    }
}
