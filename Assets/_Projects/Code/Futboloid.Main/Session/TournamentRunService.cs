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

        public int MatchesCompleted { get; private set; }
        public bool IsEliminated { get; private set; }
        public bool LastMatchWon { get; private set; }
        public int LastPlayerScore { get; private set; }
        public int LastOpponentScore { get; private set; }

        public bool IsChampion => LastMatchWon && MatchesCompleted >= _matchesToWin;
        public bool CanStartNextMatch => !IsEliminated && !IsChampion;

        public string RoundLabel => GetRoundLabel();
        public string StatusLine => GetStatusLine();

        public TournamentRunService(GameplaySettings settings)
        {
            _matchesToWin = settings.MatchesToWin;
        }

        public void ResetRun()
        {
            MatchesCompleted = 0;
            IsEliminated = false;
            LastMatchWon = false;
            LastPlayerScore = 0;
            LastOpponentScore = 0;
        }

        public void RecordMatchResult(int playerScore, int opponentScore)
        {
            LastPlayerScore = playerScore;
            LastOpponentScore = opponentScore;
            LastMatchWon = playerScore > opponentScore;
            MatchesCompleted++;

            if (!LastMatchWon)
                IsEliminated = true;
        }

        private string GetRoundLabel()
        {
            if (IsChampion)
                return "Финал пройден";

            if (IsEliminated)
                return $"Матч {MatchesCompleted}";

            return $"Матч {MatchesCompleted + 1} из {_matchesToWin}";
        }

        private string GetStatusLine()
        {
            if (IsChampion)
                return "Чемпион забега!";

            if (IsEliminated)
                return $"Счёт {LastPlayerScore}:{LastOpponentScore} — вылет из турнира";

            if (MatchesCompleted == 0)
                return "Готов к первому матчу";

            return $"Счёт {LastPlayerScore}:{LastOpponentScore} — победа! Следующий соперник ждёт.";
        }
    }
}
