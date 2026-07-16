namespace Futboloid.Core.Localization
{
    public static class LocalizationKeys
    {
        public const string RunLevelShort = "run_level_short";
        public const string PerkLevelShort = "perk_level_short";
        public const string PerkLevelLong = "perk_level_long";
        public const string BuffLabel = "buff";
        public const string DebuffLabel = "debuff";

        public const string BtnPlay = "btn_play";
        public const string BtnContinue = "btn_continue";
        public const string BtnRestart = "btn_restart";
        public const string BtnSettings = "btn_settings";
        public const string BtnBackToMenu = "btn_back_to_menu";
        public const string BtnMatch = "btn_match";
        public const string BtnNewRun = "btn_new_run";
        public const string BtnMainMenu = "btn_main_menu";

        public const string GuideStart = "guide_start";
        public const string GuideMoveLeft = "guide_move_left";
        public const string GuideMoveRight = "guide_move_right";
        public const string ContinueHint = "continue_hint";

        public const string LeaderboardTitle = "leaderboard_title";
        public const string LeaderboardSaveNick = "leaderboard_save_nick";
        public const string LeaderboardPlayerPlace = "leaderboard_player_place";
        public const string LeaderboardPlayerScore = "leaderboard_player_score";
        public const string LeaderboardNoScore = "leaderboard_no_score";
        public const string LeaderboardOffline = "leaderboard_offline";
        public const string LeaderboardLoading = "leaderboard_loading";
        public const string LeaderboardError = "leaderboard_error";
        public const string LeaderboardRefresh = "leaderboard_refresh";
        public const string LeaderboardTopTitle = "leaderboard_top_title";
        public const string LeaderboardTableRow = "leaderboard_table_row";

        public const string RoundFinalCompleted = "round_final_completed";
        public const string RoundMatch = "round_match";
        public const string RoundMatchOf = "round_match_of";
        public const string StatusChampion = "status_champion";
        public const string StatusEliminated = "status_eliminated";
        public const string StatusEliminatedConcede = "status_eliminated_concede";
        public const string StatusReadyFirstMatch = "status_ready_first_match";
        public const string StatusVictory = "status_victory";

        public static string PerkName(string perkId) => $"perk.{perkId}.name";
        public static string PerkDescription(string perkId) => $"perk.{perkId}.description";
        public static string StatusEffectName(string effectId) => $"effect.{effectId}.name";
        public static string StatusEffectDescription(string effectId) => $"effect.{effectId}.description";
    }
}
