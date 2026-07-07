using Futboloid.Core;
using Futboloid.Gameplay.Match;
using UnityEngine;

namespace Futboloid.Main.Session
{
    /// <summary>
    /// Прогресс забега в турнире (App scope). MVP: линейная цепочка матчей без полной сетки.
    /// </summary>
    public class TournamentRunService : ITournamentRunService
    {
        private readonly GameplaySettings _settings;
        private readonly int _matchesToWin;

        private int _matchesCompleted;
        private int _lastPlayerScore;
        private int _lastOpponentScore;
        private int _runSeed;

        public TournamentRunState RunState { get; private set; } = TournamentRunState.InProgress;

        public int CurrentMatchNumber => _matchesCompleted + 1;
        public int MatchesToWin => _matchesToWin;
        public int RunSeed => _runSeed;

        public string RoundLabel => GetRoundLabel();
        public string StatusLine => GetStatusLine();

        public TournamentRunService(GameplaySettings settings)
        {
            _settings = settings;
            _matchesToWin = settings.MatchesToWin;
            _runSeed = Random.Range(1, int.MaxValue);
        }

        public void ResetRun()
        {
            _lastPlayerScore = 0;
            _lastOpponentScore = 0;
            _runSeed = Random.Range(1, int.MaxValue);
            RunState = TournamentRunState.InProgress;

            var startMatch = _settings.DebugStartMatchEnabled
                ? _settings.DebugStartMatch
                : 1;
            _matchesCompleted = Mathf.Clamp(startMatch - 1, 0, _matchesToWin - 1);

            if (_settings.DebugStartMatchEnabled)
            {
                Debug.Log(
                    $"[TournamentRunService] Debug start: match {CurrentMatchNumber} / {_matchesToWin}");
            }
        }

        public void RecordMatchResult(int playerScore, int opponentScore, bool playerWon)
        {
            _lastPlayerScore = playerScore;
            _lastOpponentScore = opponentScore;
            _matchesCompleted++;

            if (!playerWon)
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
                    return $"Матч {CurrentMatchNumber}";
                default:
                    return $"Матч {CurrentMatchNumber} из {_matchesToWin}";
            }
        }

        private string GetStatusLine()
        {
            switch (RunState)
            {
                case TournamentRunState.Completed:
                    return "Чемпион забега!";
                case TournamentRunState.Eliminated:
                    return $"Счёт\n <color=red><size=150%>{_lastPlayerScore}:{_lastOpponentScore}</size></color> \n вылет из турнира";
                default:
                    if (_matchesCompleted == 0)
                        return "Готов к первому матчу";
                    return $"Счёт\n<color=red><size=150%>{_lastPlayerScore}:{_lastOpponentScore}</size></color>\n победа!";
            }
        }
    }
}
