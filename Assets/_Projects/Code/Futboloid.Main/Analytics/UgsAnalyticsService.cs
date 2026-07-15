using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Futboloid.Core.Leaderboards;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UnityConsent;
using IAnalyticsService = Futboloid.Core.Analytics.IAnalyticsService;
using AnalyticsEvent = Futboloid.Core.Analytics.AnalyticsEvent;

namespace Futboloid.Main.Analytics
{
    /// <summary>
    /// UGS Analytics (Unity 6.2+ / Analytics 6.1+).
    /// Consent через <see cref="EndUserConsent"/>; схемы событий — в Dashboard Event Manager.
    /// GDPR UI-баннер пока нет: AnalyticsIntent = Granted при старте (как анонимный login лидербордов).
    /// </summary>
    public sealed class UgsAnalyticsService : IAnalyticsService
    {
        private readonly ILeaderboardService _leaderboard;
        private readonly AsyncLazy _readyLazy;
        private bool _collectionStarted;
        private bool _initFailed;

        public UgsAnalyticsService(ILeaderboardService leaderboard)
        {
            _leaderboard = leaderboard;
            // UniTask нельзя await'ить дважды — AsyncLazy даёт общий результат многим Track().
            _readyLazy = new AsyncLazy(InitializeInternalAsync);
        }

        public void Track(AnalyticsEvent evt) => TrackAsync(evt).Forget();

        public void SetUserProperty(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            Debug.Log($"[Analytics:UGS] SetUserProperty skipped ({key}={value})");
        }

        public void Flush()
        {
            if (!_collectionStarted)
                return;

            try
            {
                AnalyticsService.Instance.Flush();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Analytics:UGS] Flush failed: {ex.Message}");
            }
        }

        private async UniTaskVoid TrackAsync(AnalyticsEvent evt)
        {
            if (string.IsNullOrEmpty(evt.Name))
                return;

            await EnsureReadyAsync();
            if (_initFailed || !_collectionStarted)
                return;

            try
            {
                Record(evt);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Analytics:UGS] Track '{evt.Name}' failed: {ex.Message}");
            }
        }

        private UniTask EnsureReadyAsync()
        {
            if (_collectionStarted || _initFailed)
                return UniTask.CompletedTask;

            return _readyLazy.Task;
        }

        private async UniTask InitializeInternalAsync()
        {
            if (!IsNetworkAvailable())
            {
                _initFailed = true;
                Debug.LogWarning("[Analytics:UGS] No network — analytics disabled for this session.");
                return;
            }

            try
            {
                if (_leaderboard != null)
                    await _leaderboard.InitializeAsync();
                else
                    await UnityServices.InitializeAsync();

                if (UnityServices.State != ServicesInitializationState.Initialized)
                    await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                // Unity 6.2+ / Analytics 6.1+: consent через Developer Data framework.
                EndUserConsent.SetConsentState(new ConsentState
                {
                    AnalyticsIntent = ConsentStatus.Granted,
                    AdsIntent = ConsentStatus.Denied
                });

                _collectionStarted = true;
                Debug.Log("[Analytics:UGS] Consent granted, data collection active.");
            }
            catch (Exception ex)
            {
                _initFailed = true;
                Debug.LogWarning($"[Analytics:UGS] Init failed: {ex.Message}");
            }
        }

        private static void Record(AnalyticsEvent evt)
        {
            if (evt.Parameters == null || evt.Parameters.Count == 0)
            {
                AnalyticsService.Instance.RecordEvent(evt.Name);
                return;
            }

            var custom = new CustomEvent(evt.Name);
            foreach (var pair in evt.Parameters)
            {
                if (string.IsNullOrEmpty(pair.Key) || pair.Value == null)
                    continue;

                custom.Add(pair.Key, ToSupportedValue(pair.Value));
            }

            AnalyticsService.Instance.RecordEvent(custom);
        }

        private static object ToSupportedValue(object value)
        {
            switch (value)
            {
                case string:
                case int:
                case long:
                case float:
                case double:
                case bool:
                case DateTime:
                    return value;
                case Enum e:
                    return e.ToString();
                default:
                    return value.ToString();
            }
        }

        private static bool IsNetworkAvailable() =>
            Application.internetReachability != NetworkReachability.NotReachable;
    }
}
