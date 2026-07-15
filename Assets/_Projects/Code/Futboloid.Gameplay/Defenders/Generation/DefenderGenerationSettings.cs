using System;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    [CreateAssetMenu(fileName = "DefenderGenerationSettings", menuName = "Futboloid/Defender Generation Settings")]
    public sealed class DefenderGenerationSettings : ScriptableObject
    {
        public const int GoalkeeperSlotId = 100;
        public const string ResourcePath = "Data/Settings/DefenderGenerationSettings";
        public const int MaxSameArchetypePerMatch = 2;
        public const int FieldCountJitter = 1;
        public const int ReferenceMatchCount = 9;
        public const int MinFieldHp = 3;
        public const int FinalFieldCount = 11;

        [SerializeField] private int gridColumns = 5;
        [SerializeField] private int gridRows = 7;
        [SerializeField] private int generationSeedSalt = 7919;

        [Header("Goalkeeper")]
        [SerializeField] private int gkBaseHp = 10;
        [SerializeField] private int gkHpPerMatch = 2;
        [SerializeField] private float gkTrackSpeed = 1.25f;

        [Header("Field HP")]
        [SerializeField] private int fieldBaseHp = 3;
        [SerializeField] private AnimationCurve pacingCurve = AnimationCurve.Linear(1f, 0.35f, 9f, 1f);

        [Header("Scoring")]
        [SerializeField] private int gkPointValue = 30;

        [Header("Field behavior defaults")]
        [SerializeField] private FieldBehaviorDefaults fieldDefaults = FieldBehaviorDefaults.CreateDefault();

        [Header("Tutorial (match 1)")]
        [SerializeField] private TutorialMatchDefinition tutorialMatch = TutorialMatchDefinition.CreateDefault();

        [Header("Placement zone")]
        [SerializeField] private PlacementZoneDefinition placementZone = PlacementZoneDefinition.CreateDefault();

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
        public FieldBehaviorDefaults FieldDefaults => fieldDefaults;
        public PlacementZoneDefinition PlacementZone => placementZone;
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
                pacingCurve = AnimationCurve.Linear(1f, 0.35f, 9f, 1f);

            if (tutorialMatch.FieldCount < 1)
                tutorialMatch = TutorialMatchDefinition.CreateDefault();

            if (formations == null || formations.Length == 0)
                formations = FormationShapeDefinition.CreateDefaults();

            if (archetypes == null || archetypes.Length == 0)
                archetypes = DefenderArchetypeDefinition.CreateDefaults();

            if (tiers == null || tiers.Length == 0)
                tiers = MatchTierDefinition.CreateDefaults();
        }

        public float EvaluatePacing(int matchNumber, int totalMatches)
        {
            if (pacingCurve == null || pacingCurve.length == 0)
                return 1f;

            var progress = ResolveMatchProgress(matchNumber, totalMatches);
            var lastTime = pacingCurve.keys[pacingCurve.length - 1].time;
            var curveTime = Mathf.Lerp(1f, lastTime, progress);
            return Mathf.Max(0.1f, pacingCurve.Evaluate(curveTime));
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
            return Mathf.Max(MinFieldHp, Mathf.RoundToInt(scaled));
        }

        public MatchTierDefinition ResolveTier(int matchNumber, int totalMatches)
        {
            var tiersList = Tiers;
            if (tiersList == null || tiersList.Length == 0)
                return MatchTierDefinition.CreateDefaults()[0];

            var scale = Mathf.Max(1f, totalMatches / (float)ReferenceMatchCount);

            for (var i = tiersList.Length - 1; i >= 0; i--)
            {
                var tier = tiersList[i];
                var scaledMin = Mathf.Max(1, Mathf.RoundToInt(tier.MinMatchNumber * scale));
                if (matchNumber >= scaledMin)
                    return tier;
            }

            return tiersList[0];
        }

        public PlacementZoneDefinition ResolvePlacementZone(int matchNumber, int totalMatches)
        {
            var zone = placementZone;
            var progress = ResolveMatchProgress(matchNumber, totalMatches);

            if (matchNumber >= totalMatches || progress >= 0.85f)
                zone.MaxRow = Mathf.Max(zone.MaxRow, 5);
            else if (progress >= 0.55f)
                zone.MaxRow = Mathf.Max(zone.MaxRow, 5);

            return zone;
        }

        public int ResolveFieldCount(int matchNumber, int totalMatches, System.Random rng)
        {
            if (matchNumber <= 1)
                return Mathf.Max(1, TutorialMatch.FieldCount);

            var baseCount = ResolveScaledBaseFieldCount(matchNumber, totalMatches);
            var jitter = rng.Next(FieldCountJitter * 2 + 1) - FieldCountJitter;
            var zone = ResolvePlacementZone(matchNumber, totalMatches);
            var maxInZone = zone.CellCount;
            return Mathf.Clamp(baseCount + jitter, 2, maxInZone);
        }

        private static float ResolveMatchProgress(int matchNumber, int totalMatches) =>
            (matchNumber - 1f) / Mathf.Max(1f, totalMatches - 1f);

        private int ResolveScaledBaseFieldCount(int matchNumber, int totalMatches)
        {
            var progress = ResolveMatchProgress(matchNumber, totalMatches);
            var startCount = TutorialMatch.FieldCount;
            var scaled = Mathf.RoundToInt(Mathf.Lerp(startCount, FinalFieldCount, progress));

            if (matchNumber >= totalMatches)
                scaled = Mathf.Max(scaled, FinalFieldCount);

            return scaled;
        }
    }

    [Serializable]
    public struct FieldBehaviorDefaults
    {
        public int PatrolPointCount;
        public float PatrolRadius;
        public float SeparationRadius;
        public float FieldAcceleration;
        public float FieldArriveThreshold;
        public float InteractionCooldown;

        public static FieldBehaviorDefaults CreateDefault() => new()
        {
            PatrolPointCount = 4,
            PatrolRadius = 2.5f,
            SeparationRadius = 0.8f,
            FieldAcceleration = 12f,
            FieldArriveThreshold = 0.12f,
            InteractionCooldown = 0.1f
        };
    }

    [Serializable]
    public struct TutorialMatchDefinition
    {
        public int FieldCount;

        public static TutorialMatchDefinition CreateDefault() => new()
        {
            FieldCount = 5
        };
    }

    [Serializable]
    public struct PlacementZoneDefinition
    {
        public int MinCol;
        public int MaxCol;
        public int MinRow;
        public int MaxRow;

        public bool Contains(int col, int row) =>
            col >= MinCol && col <= MaxCol && row >= MinRow && row <= MaxRow;

        public int CellCount =>
            Mathf.Max(0, MaxCol - MinCol + 1) * Mathf.Max(0, MaxRow - MinRow + 1);

        public static PlacementZoneDefinition CreateDefault() => new()
        {
            MinCol = 0,
            MaxCol = 4,
            MinRow = 0,
            MaxRow = 4
        };
    }

    [Serializable]
    public struct GridCellOffset
    {
        public int Col;
        public int Row;
    }

    [Serializable]
    public struct FormationShapeDefinition
    {
        public string Id;
        public GridCellOffset[] Cells;
        public int MinTier;
        public float Weight;

        public int Size => Cells != null ? Cells.Length : 0;

        public static FormationShapeDefinition[] CreateDefaults() => new[]
        {
            new FormationShapeDefinition
            {
                Id = "Dot1",
                Cells = new[] { new GridCellOffset { Col = 0, Row = 0 } },
                MinTier = 0,
                Weight = 1f
            },
            new FormationShapeDefinition
            {
                Id = "Line3",
                Cells = new[]
                {
                    new GridCellOffset { Col = -1, Row = 0 },
                    new GridCellOffset { Col = 0, Row = 0 },
                    new GridCellOffset { Col = 1, Row = 0 }
                },
                MinTier = 0,
                Weight = 2f
            },
            new FormationShapeDefinition
            {
                Id = "Triangle3",
                Cells = new[]
                {
                    new GridCellOffset { Col = 0, Row = 0 },
                    new GridCellOffset { Col = -1, Row = 1 },
                    new GridCellOffset { Col = 1, Row = 1 }
                },
                MinTier = 0,
                Weight = 3f
            },
            new FormationShapeDefinition
            {
                Id = "Diamond4",
                Cells = new[]
                {
                    new GridCellOffset { Col = 0, Row = -1 },
                    new GridCellOffset { Col = -1, Row = 0 },
                    new GridCellOffset { Col = 1, Row = 0 },
                    new GridCellOffset { Col = 0, Row = 1 }
                },
                MinTier = 1,
                Weight = 2.5f
            },
            new FormationShapeDefinition
            {
                Id = "Cross4",
                Cells = new[]
                {
                    new GridCellOffset { Col = 0, Row = 0 },
                    new GridCellOffset { Col = -1, Row = 0 },
                    new GridCellOffset { Col = 1, Row = 0 },
                    new GridCellOffset { Col = 0, Row = 1 }
                },
                MinTier = 1,
                Weight = 2f
            },
            new FormationShapeDefinition
            {
                Id = "V5",
                Cells = new[]
                {
                    new GridCellOffset { Col = -1, Row = -1 },
                    new GridCellOffset { Col = 1, Row = -1 },
                    new GridCellOffset { Col = 0, Row = 0 },
                    new GridCellOffset { Col = -1, Row = 1 },
                    new GridCellOffset { Col = 1, Row = 1 }
                },
                MinTier = 2,
                Weight = 2f
            },
            new FormationShapeDefinition
            {
                Id = "Wall5",
                Cells = new[]
                {
                    new GridCellOffset { Col = -2, Row = 0 },
                    new GridCellOffset { Col = -1, Row = 0 },
                    new GridCellOffset { Col = 0, Row = 0 },
                    new GridCellOffset { Col = 1, Row = 0 },
                    new GridCellOffset { Col = 2, Row = 0 }
                },
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
        public float WanderRadius;
        public float FieldMoveSpeed;
        public float LaunchSpeed;
        public int OpenGoalChancePercent;
        public int PointValue;

        public DefenderBuild ToBuild(int slotId, int maxHp, in FieldBehaviorDefaults defaults)
        {
            return new DefenderBuild
            {
                SlotId = slotId,
                Role = DefenderRole.Field,
                MaxHp = maxHp,
                HitType = HitType,
                MovementType = MovementType,
                PatrolPointCount = Mathf.Max(1, defaults.PatrolPointCount),
                PatrolRadius = defaults.PatrolRadius,
                WanderRadius = WanderRadius,
                SeparationRadius = defaults.SeparationRadius,
                FieldMoveSpeed = FieldMoveSpeed,
                FieldAcceleration = defaults.FieldAcceleration,
                FieldArriveThreshold = defaults.FieldArriveThreshold,
                LaunchSpeed = LaunchSpeed,
                OpenGoalChancePercent = OpenGoalChancePercent,
                InteractionCooldown = defaults.InteractionCooldown,
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
                Weight = 3.5f,
                Hp = 3,
                PointValue = 10,
                HitType = DefenderHitType.Reflect,
                MovementType = DefenderMovementType.Idle,
                WanderRadius = 2.5f,
                FieldMoveSpeed = 1.4f,
                LaunchSpeed = 12f,
                OpenGoalChancePercent = 70
            },
            new DefenderArchetypeDefinition
            {
                Id = "Drifter",
                MinTier = 0,
                Weight = 3.5f,
                Hp = 2,
                PointValue = 8,
                HitType = DefenderHitType.Reflect,
                MovementType = DefenderMovementType.WanderInRadius,
                WanderRadius = 3.5f,
                FieldMoveSpeed = 1.5f,
                LaunchSpeed = 12f,
                OpenGoalChancePercent = 70
            },
            new DefenderArchetypeDefinition
            {
                Id = "Hunter",
                MinTier = 1,
                Weight = 2f,
                Hp = 3,
                PointValue = 12,
                HitType = DefenderHitType.Reflect,
                MovementType = DefenderMovementType.ChaseBall,
                WanderRadius = 3f,
                FieldMoveSpeed = 1.7f,
                LaunchSpeed = 12f,
                OpenGoalChancePercent = 70
            },
            new DefenderArchetypeDefinition
            {
                Id = "Sniper",
                MinTier = 1,
                Weight = 1f,
                Hp = 2,
                PointValue = 15,
                HitType = DefenderHitType.ToPlayerGoal,
                MovementType = DefenderMovementType.Idle,
                WanderRadius = 2.5f,
                FieldMoveSpeed = 1.3f,
                LaunchSpeed = 13f,
                OpenGoalChancePercent = 45
            },
            new DefenderArchetypeDefinition
            {
                Id = "Tank",
                MinTier = 2,
                Weight = 1.5f,
                Hp = 5,
                PointValue = 25,
                HitType = DefenderHitType.Reflect,
                MovementType = DefenderMovementType.Idle,
                WanderRadius = 2.5f,
                FieldMoveSpeed = 1.2f,
                LaunchSpeed = 11f,
                OpenGoalChancePercent = 60
            },
            new DefenderArchetypeDefinition
            {
                Id = "Striker",
                MinTier = 2,
                Weight = 1f,
                Hp = 3,
                PointValue = 14,
                HitType = DefenderHitType.ToPlayerGoal,
                MovementType = DefenderMovementType.ChaseBall,
                WanderRadius = 3f,
                FieldMoveSpeed = 1.65f,
                LaunchSpeed = 12.5f,
                OpenGoalChancePercent = 40
            },
            new DefenderArchetypeDefinition
            {
                Id = "Presser",
                MinTier = 3,
                Weight = 0.8f,
                Hp = 3,
                PointValue = 18,
                HitType = DefenderHitType.ToPlayerGoal,
                MovementType = DefenderMovementType.ChaseBall,
                WanderRadius = 3f,
                FieldMoveSpeed = 1.8f,
                LaunchSpeed = 13f,
                OpenGoalChancePercent = 45
            }
        };
    }

    [Serializable]
    public struct MatchTierDefinition
    {
        public int MinMatchNumber;
        public int Tier;
        public int BaseFieldCount;

        public static MatchTierDefinition[] CreateDefaults() => new[]
        {
            new MatchTierDefinition { MinMatchNumber = 1, Tier = 0, BaseFieldCount = 5 },
            new MatchTierDefinition { MinMatchNumber = 2, Tier = 1, BaseFieldCount = 5 },
            new MatchTierDefinition { MinMatchNumber = 4, Tier = 2, BaseFieldCount = 6 },
            new MatchTierDefinition { MinMatchNumber = 6, Tier = 3, BaseFieldCount = 8 },
            new MatchTierDefinition { MinMatchNumber = 8, Tier = 4, BaseFieldCount = 10 },
            new MatchTierDefinition { MinMatchNumber = 9, Tier = 4, BaseFieldCount = 11 }
        };
    }
}
