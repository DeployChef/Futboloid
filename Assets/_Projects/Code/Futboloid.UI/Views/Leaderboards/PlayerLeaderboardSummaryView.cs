using Futboloid.Core.Leaderboards;
using Futboloid.Core.Localization;
using TMPro;
using UnityEngine;
using VContainer;

namespace Futboloid.UI.Views.Leaderboards
{
    /// <summary>
    /// Верхний блок: место игрока и очки (отдельные тексты для будущей анимации).
    /// </summary>
    public sealed class PlayerLeaderboardSummaryView : MonoBehaviour, ILeaderboardSnapshotView
    {
        [SerializeField] private TextMeshProUGUI placeText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI statusText;

        private ILocalizationService _localization;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization;
        }

        public void ApplyLoading()
        {
            SetStatus(GetText(LocalizationKeys.LeaderboardLoading));
            SetPlaceVisible(false);
            SetScoreVisible(false);
        }

        public void ApplyOffline()
        {
            SetStatus(GetText(LocalizationKeys.LeaderboardOffline));
            SetPlaceVisible(false);
            SetScoreVisible(false);
        }

        public void ApplyError()
        {
            SetStatus(GetText(LocalizationKeys.LeaderboardError));
            SetPlaceVisible(false);
            SetScoreVisible(false);
        }

        public void ApplySnapshot(LeaderboardSnapshot snapshot)
        {
            SetStatusVisible(false);

            if (!snapshot.HasPlayerEntry)
            {
                SetPlaceVisible(true);
                SetScoreVisible(false);

                if (placeText != null)
                    placeText.text = GetText(LocalizationKeys.LeaderboardNoScore);

                return;
            }

            var entry = snapshot.PlayerEntry;
            var total = Mathf.Max(snapshot.TotalPlayers, entry.Rank);

            SetPlaceVisible(true);
            SetScoreVisible(true);

            if (placeText != null)
            {
                placeText.text = GetText(
                    LocalizationKeys.LeaderboardPlayerPlace,
                    entry.Rank,
                    total);
            }

            if (scoreText != null)
            {
                scoreText.text = GetText(
                    LocalizationKeys.LeaderboardPlayerScore,
                    entry.Score);
            }
        }

        private void SetStatus(string message)
        {
            SetStatusVisible(true);
            SetPlaceVisible(false);
            SetScoreVisible(false);

            if (statusText != null)
                statusText.text = message;
        }

        private void SetStatusVisible(bool visible)
        {
            if (statusText != null)
                statusText.gameObject.SetActive(visible);
        }

        private void SetPlaceVisible(bool visible)
        {
            if (placeText != null)
                placeText.gameObject.SetActive(visible);
        }

        private void SetScoreVisible(bool visible)
        {
            if (scoreText != null)
                scoreText.gameObject.SetActive(visible);
        }

        private string GetText(string key, params object[] args)
        {
            if (_localization == null || !_localization.IsReady)
                return key;

            return args == null || args.Length == 0
                ? _localization.Get(LocalizationTables.UI, key)
                : _localization.Get(LocalizationTables.UI, key, args);
        }
    }
}
