using UnityEngine;

namespace Futboloid.Gameplay.Physics
{
    public static class PhysicsLayers
    {
        public const string Wall = "Wall";
        public const string Keeper = "Keeper";
        public const string Defender = "Defender";
        public const string GoalEnemy = "GoalEnemy";
        public const string GoalPlayer = "GoalPlayer";

        public static int WallId => LayerMask.NameToLayer(Wall);
        public static int KeeperId => LayerMask.NameToLayer(Keeper);
        public static int GoalEnemyId => LayerMask.NameToLayer(GoalEnemy);
        public static int GoalPlayerId => LayerMask.NameToLayer(GoalPlayer);
        public static int DefenderId => LayerMask.NameToLayer(Defender);

        public static LayerMask BallContactMask => LayerMask.GetMask(Wall, Keeper, Defender);

        public static LayerMask GoalMask => LayerMask.GetMask(GoalEnemy, GoalPlayer);

        public static int GoalEnemyMask => 1 << GoalEnemyId;

        public static int GoalPlayerMask => 1 << GoalPlayerId;
    }
}
