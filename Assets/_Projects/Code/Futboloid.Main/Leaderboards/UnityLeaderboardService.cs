using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Futboloid.Core.Leaderboards;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace Futboloid.Main.Leaderboards
{
    public sealed class UnityLeaderboardService : ILeaderboardService
    {
        private readonly IPlayerNicknameStore _nicknameStore;
        private UniTask? _initializeTask;
        private bool _isInitialized;
        private bool _isSignedIn;

        public UnityLeaderboardService(IPlayerNicknameStore nicknameStore)
        {
            _nicknameStore = nicknameStore;
        }

        public bool IsInitialized => _isInitialized;
        public bool IsSignedIn => _isSignedIn;

        public UniTask InitializeAsync()
        {
            if (_isInitialized)
                return UniTask.CompletedTask;

            _initializeTask ??= InitializeInternalAsync();
            return _initializeTask.Value;
        }

        public async UniTask SetNicknameAsync(string nickname)
        {
            var normalized = PlayerNicknameStore.Normalize(nickname);
            if (string.IsNullOrEmpty(normalized))
                return;

            _nicknameStore.Save(normalized);

            if (!IsNetworkAvailable())
                return;

            await EnsureReadyAsync();

            if (!_isSignedIn)
                return;

            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(normalized);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Leaderboard] Failed to update player name: {ex.Message}");
            }
        }

        public async UniTask SubmitRunScoreAsync(int score)
        {
            if (score <= 0)
                return;

            if (!IsNetworkAvailable())
                return;

            await EnsureReadyAsync();

            if (!_isSignedIn)
                return;

            try
            {
                await GetLeaderboards().AddPlayerScoreAsync(
                    LeaderboardIds.ArcadeScore,
                    score);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Leaderboard] Failed to submit score {score}: {ex.Message}");
            }
        }

        public async UniTask<LeaderboardSnapshot> FetchSnapshotAsync(int topCount = 10)
        {
            if (!IsNetworkAvailable())
            {
                return new LeaderboardSnapshot
                {
                    Status = LeaderboardStatus.Offline,
                };
            }

            await EnsureReadyAsync();

            if (!_isSignedIn)
            {
                return new LeaderboardSnapshot
                {
                    Status = LeaderboardStatus.Error,
                };
            }

            try
            {
                var limit = Mathf.Clamp(topCount, 1, 25);
                var scoresPage = await GetLeaderboards().GetScoresAsync(
                    LeaderboardIds.ArcadeScore,
                    new GetScoresOptions { Limit = limit });

                var topEntries = MapEntries(scoresPage.Results);
                var hasPlayerEntry = false;
                LeaderboardEntryDto playerEntry = default;

                try
                {
                    var playerScore = await GetLeaderboards().GetPlayerScoreAsync(
                        LeaderboardIds.ArcadeScore);
                    playerEntry = MapEntry(playerScore);
                    hasPlayerEntry = true;
                }
                catch (Exception ex)
                {
                    Debug.Log($"[Leaderboard] Player has no score yet: {ex.Message}");
                }

                return new LeaderboardSnapshot
                {
                    Status = LeaderboardStatus.Ready,
                    TopEntries = topEntries,
                    HasPlayerEntry = hasPlayerEntry,
                    PlayerEntry = playerEntry,
                    TotalPlayers = scoresPage.Total,
                };
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Leaderboard] Failed to fetch snapshot: {ex.Message}");
                return new LeaderboardSnapshot
                {
                    Status = LeaderboardStatus.Error,
                };
            }
        }

        private async UniTask InitializeInternalAsync()
        {
            if (!IsNetworkAvailable())
            {
                _isInitialized = true;
                return;
            }

            try
            {
                await UnityServices.InitializeAsync();
                _isInitialized = true;

                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                _isSignedIn = AuthenticationService.Instance.IsSignedIn;

                var nickname = _nicknameStore.Nickname;
                if (_isSignedIn && !string.IsNullOrEmpty(nickname))
                {
                    try
                    {
                        await AuthenticationService.Instance.UpdatePlayerNameAsync(nickname);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[Leaderboard] Failed to apply saved nickname: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _isInitialized = true;
                Debug.LogWarning($"[Leaderboard] Initialization failed: {ex.Message}");
            }
        }

        private async UniTask EnsureReadyAsync()
        {
            await InitializeAsync();

            if (_isSignedIn || !IsNetworkAvailable())
                return;

            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                _isSignedIn = AuthenticationService.Instance.IsSignedIn;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Leaderboard] Sign-in failed: {ex.Message}");
            }
        }

        private static bool IsNetworkAvailable() =>
            Application.internetReachability != NetworkReachability.NotReachable;

        private static ILeaderboardsService GetLeaderboards() =>
            UnityServices.Instance.GetLeaderboardsService();

        private static IReadOnlyList<LeaderboardEntryDto> MapEntries(IReadOnlyList<LeaderboardEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return Array.Empty<LeaderboardEntryDto>();

            var mapped = new List<LeaderboardEntryDto>(entries.Count);
            foreach (var entry in entries)
                mapped.Add(MapEntry(entry));

            return mapped;
        }

        private static LeaderboardEntryDto MapEntry(LeaderboardEntry entry) =>
            new(
                LeaderboardDisplayNames.ToDisplayRank(entry.Rank),
                LeaderboardDisplayNames.StripUnitySuffix(entry.PlayerName),
                Mathf.RoundToInt((float)entry.Score));
    }
}
