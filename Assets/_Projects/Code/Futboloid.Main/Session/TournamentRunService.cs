using Futboloid.Core;
using Futboloid.Gameplay.Match;

namespace Futboloid.Main.Session
{
    /// <summary>
    /// Прогресс забега в турнире (App scope). MVP: линейная цепочка матчей без полной сетки.
    /// </summary>
    public class TournamentRunService : ITournamentRunService
    {
        private readonly int _matchesToWin;

        private int _matchesCompleted;
        private int _lastPlayerScore;
        private int _lastOpponentScore;

        public TournamentRunState RunState { get; private set; } = TournamentRunState.InProgress;

        public int CurrentMatchNumber => _matchesCompleted + 1;

        public string RoundLabel => GetRoundLabel();
        public string StatusLine => GetStatusLine();

        public TournamentRunService(GameplaySettings settings)
        {
            _matchesToWin = settings.MatchesToWin;
        }

        public void ResetRun()
        {
            _matchesCompleted = 0;
            _lastPlayerScore = 0;
            _lastOpponentScore = 0;
            RunState = TournamentRunState.InProgress;
        }

        public void RecordMatchResult(int playerScore, int opponentScore)
        {
            _lastPlayerScore = playerScore;
            _lastOpponentScore = opponentScore;
            var wonMatch = playerScore > opponentScore;
            _matchesCompleted++;

            if (!wonMatch)
                RunState = TournamentRunState.Eliminated;
            else if (_matchesCompleted >= _matchesToWin)
                RunState = TournamentRunState.Completed;
        }

        private string GetRoundLabel()
        {
            switch (RunState)
            {
                case TournamentRunState.Completed:
                    return "Финал пройден";
                case TournamentRunState.Eliminated:
                    return $"Матч {_matchesCompleted}";
                default:
                    return $"Матч {_matchesCompleted + 1} из {_matchesToWin}";
            }
        }

        private string GetStatusLine()
        {
            switch (RunState)
            {
                case TournamentRunState.Completed:
                    return "Чемпион забега!";
                case TournamentRunState.Eliminated:
                    return $"Счёт {_lastPlayerScore}:{_lastOpponentScore} — вылет из турнира";
                default:
                    if (_matchesCompleted == 0)
                        return "Готов к первому матчу";
                    return $"Счёт {_lastPlayerScore}:{_lastOpponentScore} — победа! Следующий соперник ждёт.";
            }
        }
    }
}
