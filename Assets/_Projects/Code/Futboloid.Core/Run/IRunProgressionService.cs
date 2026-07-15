namespace Futboloid.Core.Run
{
    public interface IRunProgressionService
    {
        int CurrentXp { get; }
        int RunLevel { get; }
        int XpToNextLevel { get; }
        float XpFill01 { get; }
        int PendingPerkPicks { get; }
        bool IsBonusPickActive { get; }

        float GetGoalkeeperMoveSpeedMultiplier();

        float GetGoalkeeperWidthMultiplier();

        float GetGoalkeeperKickMultiplier();

        int GetBallDamageBonus();

        float GetEnemyHpMultiplier();

        int GetComboMinMultiplier();

        float GetComboDecayIntervalMultiplier();

        int GetPerkLevel(string perkId);

        void Reset();

        void ApplyPerkPick(string perkId);

        /// <summary>Повторно отправить состояние на HUD (после инжекта виджета).</summary>
        void NotifyHud();
    }
}
