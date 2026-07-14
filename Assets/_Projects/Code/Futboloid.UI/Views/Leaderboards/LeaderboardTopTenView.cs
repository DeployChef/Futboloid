using System.Collections.Generic;
using System.Text;
using Futboloid.Core.Leaderboards;
using Futboloid.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.Leaderboards
{
    /// <summary>
    /// Нижний блок: топ-10 и кнопка обновления.
    /// </summary>
    public sealed class LeaderboardTopTenView : MonoBehaviour, ILeaderboardSnapshotView
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI entriesText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button refreshButton;
        [SerializeField] private LeaderboardRefreshHub refreshHub;

        private ILocalizationService _localization;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization;
        }

        private void Awake()
        {
            if (refreshHub == null)
                refreshHub = GetComponentInParent<LeaderboardRefreshHub>();

            if (refreshButton != null)
                refreshButton.onClick.AddListener(OnRefreshClicked);
        }

        private void OnEnable()
        {
            ApplyStaticTexts();
        }

        private void OnDestroy()
        {
            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(OnRefreshClicked);
        }

        public void ApplyLoading()
        {
            SetStatus(GetText(LocalizationKeys.LeaderboardLoading));
            SetEntriesVisible(false);
        }

        public void ApplyOffline()
        {
            SetStatus(GetText(LocalizationKeys.LeaderboardOffline));
            SetEntriesVisible(false);
        }

        public void ApplyError()
        {
            SetStatus(GetText(LocalizationKeys.LeaderboardError));
            SetEntriesVisible(false);
        }

        public void ApplySnapshot(LeaderboardSnapshot snapshot)
        {
            SetStatusVisible(false);
            SetEntriesVisible(true);
            ApplyEntries(snapshot.TopEntries);
        }

        private void OnRefreshClicked()
        {
            if (refreshHub != null)
                refreshHub.Refresh();
        }

        private void ApplyEntries(IReadOnlyList<LeaderboardEntryDto> entries)
        {
            if (entriesText == null)
                return;

            if (entries == null || entries.Count == 0)
            {
                entriesText.text = string.Empty;
                return;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (i > 0)
                    builder.AppendLine();

                builder.Append(GetText(
                    LocalizationKeys.LeaderboardTableRow,
                    entry.Rank,
                    entry.PlayerName,
                    entry.Score));
            }

            entriesText.text = builder.ToString();
        }

        private void ApplyStaticTexts()
        {
            if (titleText != null)
                titleText.text = GetText(LocalizationKeys.LeaderboardTopTitle);

            if (refreshButton != null)
            {
                var label = refreshButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = GetText(LocalizationKeys.LeaderboardRefresh);
            }
        }

        private void SetStatus(string message)
        {
            SetStatusVisible(true);
            SetEntriesVisible(false);

            if (statusText != null)
                statusText.text = message;
        }

        private void SetStatusVisible(bool visible)
        {
            if (statusText != null)
                statusText.gameObject.SetActive(visible);
        }

        private void SetEntriesVisible(bool visible)
        {
            if (entriesText != null)
                entriesText.gameObject.SetActive(visible);
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
