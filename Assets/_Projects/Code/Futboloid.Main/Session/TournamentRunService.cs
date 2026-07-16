using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Localization;
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
        private readonly ILocalizationService _localization;
        private readonly IGameEventBus _bus;
        private readonly int _matchesToWin;

        private int _matchesCompleted;
        private int _lastPlayerScore;
        private int _lastOpponentScore;
        private int _runSeed;
        private bool _hasPlayedBefore;

        public TournamentRunState RunState { get; private set; } = TournamentRunState.InProgress;

        /// <summary>Флаг первого запуска игры (не сбрасывается при рестарте).</summary>
        public bool HasPlayedBefore => _hasPlayedBefore;

        public int CurrentMatchNumber => _matchesCompleted + 1;
        public int MatchesToWin => _matchesToWin;
        public int RunSeed => _runSeed;

        public string RoundLabel => GetRoundLabel();
        public string StatusLine => GetStatusLine();

        public TournamentRunService(
            GameplaySettings settings,
            ILocalizationService localization,
            IGameEventBus bus)
        {
            _settings = settings;
            _localization = localization;
            _bus = bus;
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

            // Флаг первого запуска не сбрасываем — он перманентный для сессии
            if (_matchesCompleted >= 0)
                _hasPlayedBefore = true;

            if (_settings.DebugStartMatchEnabled)
            {
                Debug.Log(
                    $"[TournamentRunService] Debug start: match {CurrentMatchNumber} / {_matchesToWin}");
            }

            _bus.Publish(new TournamentRunStartedEvent(
                _matchesToWin,
                _runSeed,
                CurrentMatchNumber));
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
                    return _localization.Get(
                        LocalizationTables.Tournament,
                        LocalizationKeys.RoundFinalCompleted);
                case TournamentRunState.Eliminated:
                    return _localization.Get(
                        LocalizationTables.Tournament,
                        LocalizationKeys.RoundMatch,
                        CurrentMatchNumber);
                default:
                    return _localization.Get(
                        LocalizationTables.Tournament,
                        LocalizationKeys.RoundMatchOf,
                        CurrentMatchNumber,
                        _matchesToWin);
            }
        }

        private string GetStatusLine()
        {
            switch (RunState)
            {
                case TournamentRunState.Completed:
                    return _localization.Get(
                        LocalizationTables.Tournament,
                        LocalizationKeys.StatusChampion);
                case TournamentRunState.Eliminated:
                    return _localization.Get(
                        LocalizationTables.Tournament,
                        LocalizationKeys.StatusEliminated,
                        _lastPlayerScore,
                        _lastOpponentScore);
                default:
                    if (_matchesCompleted == 0)
                    {
                        return _localization.Get(
                            LocalizationTables.Tournament,
                            LocalizationKeys.StatusReadyFirstMatch);
                    }

                    return _localization.Get(
                        LocalizationTables.Tournament,
                        LocalizationKeys.StatusVictory,
                        _lastPlayerScore,
                        _lastOpponentScore);
            }
        }
    }
}
