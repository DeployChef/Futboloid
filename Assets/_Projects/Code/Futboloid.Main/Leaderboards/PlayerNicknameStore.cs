using Futboloid.Core.Leaderboards;
using UnityEngine;

namespace Futboloid.Main.Leaderboards
{
    public sealed class PlayerNicknameStore : IPlayerNicknameStore
    {
        private const string PlayerPrefsKey = "futboloid.nickname";
        private const int MaxLength = 32;

        public string Nickname => PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);

        public void Save(string nickname)
        {
            var trimmed = Normalize(nickname);
            PlayerPrefs.SetString(PlayerPrefsKey, trimmed);
            PlayerPrefs.Save();
        }

        public static string Normalize(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                return string.Empty;

            var trimmed = nickname.Trim();
            return trimmed.Length <= MaxLength ? trimmed : trimmed.Substring(0, MaxLength);
        }
    }
}
