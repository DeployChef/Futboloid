using System.Linq;

namespace Futboloid.Core.Leaderboards
{
    public static class LeaderboardDisplayNames
    {
        /// <summary>
        /// Unity всегда добавляет к нику суффикс #1234 для уникальности на сервере.
        /// В UI показываем имя без суффикса.
        /// </summary>
        public static string StripUnitySuffix(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return "—";

            var hashIndex = playerName.LastIndexOf('#');
            if (hashIndex <= 0)
                return playerName;

            var suffix = playerName.Substring(hashIndex + 1);
            if (suffix.Length > 0 && suffix.All(char.IsDigit))
                return playerName.Substring(0, hashIndex);

            return playerName;
        }

        /// <summary>UGS возвращает rank с нуля: 0 = 1-е место.</summary>
        public static int ToDisplayRank(int apiRank) => apiRank + 1;
    }
}
