using System;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Leaderboards;
using Futboloid.Gameplay.Match;
using Cysharp.Threading.Tasks;

namespace Futboloid.Main.Leaderboards
{
    public sealed class LeaderboardRunScoreSubmitter : IDisposable
    {
        private readonly ILeaderboardService _leaderboard;
        private readonly ComboScoreService _comboScore;
        private readonly ITournamentBracketReadModel _tournament;
        private readonly IDisposable _subscription;

        public LeaderboardRunScoreSubmitter(
            IGameEventBus bus,
            ILeaderboardService leaderboard,
            ComboScoreService comboScore,
            ITournamentBracketReadModel tournament)
        {
            _leaderboard = leaderboard;
            _comboScore = comboScore;
            _tournament = tournament;
            _subscription = bus.Subscribe<MatchEndedEvent>(OnMatchEnded);
        }

        public void Dispose() => _subscription?.Dispose();

        private void OnMatchEnded(MatchEndedEvent e)
        {
            if (!IsRunEnded(e))
                return;

            _leaderboard.SubmitRunScoreAsync(_comboScore.TotalScore).Forget();
        }

        private bool IsRunEnded(MatchEndedEvent e)
        {
            if (!e.PlayerWon)
                return true;

            return _tournament.CurrentMatchNumber >= _tournament.MatchesToWin;
        }
    }
}
