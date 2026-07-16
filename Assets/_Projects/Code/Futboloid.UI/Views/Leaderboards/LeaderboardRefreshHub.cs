using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Futboloid.Core.Leaderboards;
using UnityEngine;

namespace Futboloid.UI.Views.Leaderboards
{
    /// <summary>
    /// Загружает данные лидерборда один раз и раздаёт дочерним view.
    /// </summary>
    public sealed class LeaderboardRefreshHub : MonoBehaviour
    {
        [SerializeField] private PlayerLeaderboardSummaryView summaryView;
        [SerializeField] private LeaderboardTopTenView topTenView;
        [SerializeField] private PlayerNicknameControl nicknameControl;
        [SerializeField] private int topCount = 10;

        private readonly List<ILeaderboardSnapshotView> _views = new();
        private ILeaderboardService _leaderboard;
        private bool _isRefreshing;

        [VContainer.Inject]
        public void Construct(ILeaderboardService leaderboard)
        {
            _leaderboard = leaderboard;
        }

        private void Awake()
        {
            if (summaryView == null)
                summaryView = GetComponentInChildren<PlayerLeaderboardSummaryView>(true);

            if (topTenView == null)
                topTenView = GetComponentInChildren<LeaderboardTopTenView>(true);

            if (nicknameControl == null)
                nicknameControl = GetComponentInChildren<PlayerNicknameControl>(true);

            _views.Clear();

            if (summaryView != null)
                _views.Add(summaryView);

            if (topTenView != null)
                _views.Add(topTenView);
        }

        private void OnEnable()
        {
            if (nicknameControl != null)
                nicknameControl.NicknameSaved += OnNicknameSaved;

            Refresh();
        }

        private void OnDisable()
        {
            if (nicknameControl != null)
                nicknameControl.NicknameSaved -= OnNicknameSaved;
        }

        public void Refresh()
        {
            RefreshAsync().Forget();
        }

        private void OnNicknameSaved() => Refresh();

        private async UniTaskVoid RefreshAsync()
        {
            if (_leaderboard == null || _isRefreshing)
                return;

            _isRefreshing = true;
            ApplyLoading();

            var snapshot = await _leaderboard.FetchSnapshotAsync(topCount);

            switch (snapshot.Status)
            {
                case LeaderboardStatus.Offline:
                    ApplyOffline();
                    break;
                case LeaderboardStatus.Error:
                    ApplyError();
                    break;
                case LeaderboardStatus.Ready:
                    ApplySnapshot(snapshot);
                    break;
                default:
                    ApplyLoading();
                    break;
            }

            _isRefreshing = false;
        }

        private void ApplyLoading()
        {
            foreach (var view in _views)
                view.ApplyLoading();
        }

        private void ApplyOffline()
        {
            foreach (var view in _views)
                view.ApplyOffline();
        }

        private void ApplyError()
        {
            foreach (var view in _views)
                view.ApplyError();
        }

        private void ApplySnapshot(LeaderboardSnapshot snapshot)
        {
            foreach (var view in _views)
                view.ApplySnapshot(snapshot);
        }
    }
}
