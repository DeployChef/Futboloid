using System.Threading;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Run;
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
        private bool _timerStarted;
        private bool _pauseTimer;
        private float _totalDurationSeconds;

        public int PlayerScore { get; private set; }
        public int OpponentScore { get; private set; }
        public float RemainingSeconds { get; private set; }
        public float NormalizedTime =>
            _totalDurationSeconds > 0f ? RemainingSeconds / _totalDurationSeconds : 0f;

        public bool IsOnField => _onField;
        public bool WipeVictoryPending { get; private set; }

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
            _bus.Subscribe<BallServedEvent>(OnBallServed);
        }

        public void Reset()
        {
            StopTimerLoop();

            PlayerScore = 0;
            OpponentScore = 0;
            RemainingSeconds = _matchDurationSeconds;
            _totalDurationSeconds = _matchDurationSeconds;
            _matchEnded = false;
            _timerStarted = false;
            WipeVictoryPending = false;

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
                EndMatchByTime();
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

                    if (!_onField || _pauseTimer)
                        continue;

                    RemainingSeconds = Mathf.Max(0f, RemainingSeconds - Time.deltaTime);
                    PublishTimer();

                    if (RemainingSeconds <= 0f)
                        EndMatchByTime();
                }
            }
            catch (System.OperationCanceledException)
            {
                // Пауза / сброс матча.
            }
        }

        public void EndMatchFromWipe()
        {
            if (_matchEnded)
                return;

            Debug.Log("[MatchFlow] All defenders eliminated — player wins.");
            EndMatch(playerWon: true);
        }

        public void MarkWipeVictoryPending()
        {
            if (_matchEnded)
                return;

            WipeVictoryPending = true;
        }

        public bool TryCompleteWipeVictory(IRunProgressionService run)
        {
            if (!WipeVictoryPending || _matchEnded)
                return false;

            if (run != null && (run.PendingPerkPicks > 0 || run.IsBonusPickActive))
                return false;

            WipeVictoryPending = false;
            EndMatchFromWipe();
            return true;
        }

        private void EndMatch(bool playerWon)
        {
            if (_matchEnded)
                return;

            _matchEnded = true;
            StopTimerLoop();
            _bus.Publish(new MatchEndedEvent(PlayerScore, OpponentScore, playerWon));
            Debug.Log($"[MatchFlow] Match ended {PlayerScore}:{OpponentScore}, playerWon={playerWon}");
        }

        private void EndMatchByTime() => EndMatch(playerWon: PlayerScore > OpponentScore);

        private void OnGoalScored(GoalScoredEvent e) => RecordGoal(e.IsPlayerGoal);

        private void OnTimeAdjusted(MatchTimeAdjustedEvent e) =>
            AdjustTime(e.DeltaSeconds, e.Reason);

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            var wasOnField = _onField;
            _onField = e.Current == NavigationState.OnField;

            if (_onField && !_matchEnded && _timerStarted)
                StartTimerLoop();
            else if (wasOnField)
                StopTimerLoop();
        }

        private void OnBallServed(BallServedEvent _)
        {
            if (_matchEnded || _timerStarted)
                return;

            _timerStarted = true;

            if (_onField)
                StartTimerLoop();
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            _pauseTimer = e.Phase == PitchPhase.BonusPick;

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
