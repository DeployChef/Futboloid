using System.Threading;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    /// <summary>
    /// Счёт и таймер матча. Таймер — одна фоновая корутина: публикует обновления на шину,
    /// внешний код только слушает события и может сдвинуть время через <see cref="MatchTimeAdjustedEvent"/>.
    /// </summary>
    public class MatchFlow
    {
        private readonly IGameEventBus _bus;
        private readonly float _matchDurationSeconds;

        private CancellationTokenSource _timerCts;
        private bool _onField;
        private bool _matchEnded;
        private float _totalDurationSeconds;

        public int PlayerScore { get; private set; }
        public int OpponentScore { get; private set; }
        public float RemainingSeconds { get; private set; }
        public float NormalizedTime =>
            _totalDurationSeconds > 0f ? RemainingSeconds / _totalDurationSeconds : 0f;

        public MatchFlow(IGameEventBus bus, GameplaySettings settings)
        {
            _bus = bus;
            _matchDurationSeconds = settings.MatchDurationSeconds;
            _totalDurationSeconds = _matchDurationSeconds;
            RemainingSeconds = _matchDurationSeconds;

            _bus.Subscribe<GoalScoredEvent>(OnGoalScored);
            _bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged);
            _bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged);
            _bus.Subscribe<MatchTimeAdjustedEvent>(OnTimeAdjusted);
        }

        public void Reset()
        {
            StopTimerLoop();

            PlayerScore = 0;
            OpponentScore = 0;
            RemainingSeconds = _matchDurationSeconds;
            _totalDurationSeconds = _matchDurationSeconds;
            _matchEnded = false;

            PublishScore();
            PublishTimer();
        }

        public void RecordGoal(bool isPlayerGoal)
        {
            if (isPlayerGoal)
                PlayerScore++;
            else
                OpponentScore++;

            PublishScore();
            Debug.Log($"[MatchFlow] Score {PlayerScore}:{OpponentScore} (player goal={isPlayerGoal})");
        }

        public void AdjustTime(float deltaSeconds, string reason = null)
        {
            if (_matchEnded)
                return;

            RemainingSeconds = Mathf.Max(0f, RemainingSeconds + deltaSeconds);

            if (deltaSeconds > 0f)
                _totalDurationSeconds += deltaSeconds;

            PublishTimer();

            if (!string.IsNullOrEmpty(reason))
                Debug.Log($"[MatchFlow] Time adjusted {deltaSeconds:+#;-#;0}s ({reason}), remaining {RemainingSeconds:F0}s");

            if (RemainingSeconds <= 0f)
                EndMatch();
        }

        private void StartTimerLoop()
        {
            if (_matchEnded || _timerCts != null)
                return;

            _timerCts = new CancellationTokenSource();
            RunTimerLoopAsync(_timerCts.Token).Forget();
        }

        private void StopTimerLoop()
        {
            if (_timerCts == null)
                return;

            _timerCts.Cancel();
            _timerCts.Dispose();
            _timerCts = null;
        }

        private async UniTaskVoid RunTimerLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && !_matchEnded && RemainingSeconds > 0f)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);

                    if (!_onField)
                        continue;

                    RemainingSeconds = Mathf.Max(0f, RemainingSeconds - Time.deltaTime);
                    PublishTimer();

                    if (RemainingSeconds <= 0f)
                        EndMatch();
                }
            }
            catch (System.OperationCanceledException)
            {
                // Пауза / сброс матча.
            }
        }

        private void EndMatch()
        {
            if (_matchEnded)
                return;

            _matchEnded = true;
            StopTimerLoop();
            _bus.Publish(new MatchEndedEvent(PlayerScore, OpponentScore));
            Debug.Log($"[MatchFlow] Match ended {PlayerScore}:{OpponentScore}");
        }

        private void OnGoalScored(GoalScoredEvent e) => RecordGoal(e.IsPlayerGoal);

        private void OnTimeAdjusted(MatchTimeAdjustedEvent e) =>
            AdjustTime(e.DeltaSeconds, e.Reason);

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            var wasOnField = _onField;
            _onField = e.Current == NavigationState.OnField;

            if (_onField && !_matchEnded)
                StartTimerLoop();
            else if (wasOnField)
                StopTimerLoop();
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            if (e.Phase == PitchPhase.MatchEnded)
            {
                _matchEnded = true;
                StopTimerLoop();
            }
        }

        private void PublishTimer() =>
            _bus.Publish(new MatchTimerChangedEvent(RemainingSeconds, NormalizedTime));

        private void PublishScore() =>
            _bus.Publish(new MatchScoreChangedEvent(PlayerScore, OpponentScore));
    }
}
