namespace Futboloid.Core.Analytics
{
    /// <summary>Стабильные имена для дашборда. Менять только вместе с Wiki «Аналитика».</summary>
    public static class AnalyticsEventNames
    {
        public const string SessionStart = "session_start";
        public const string SessionEnd = "session_end";
        public const string TournamentStart = "tournament_start";
        public const string TournamentEnd = "tournament_end";
        public const string MatchEnd = "match_end";
        public const string PerkOffered = "perk_offered";
        public const string PerkPicked = "perk_picked";
        public const string StatusEffectApplied = "status_effect_applied";
    }
}
