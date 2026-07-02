using System;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Main.Navigation;

namespace Futboloid.Main.Navigation
{
    public class MatchEndHandler : IDisposable
    {
        private readonly OverlayStateController _overlay;
        private readonly ITournamentRunService _tournamentRun;
        private readonly IDisposable _subscription;

        public MatchEndHandler(
            IGameEventBus bus,
            OverlayStateController overlay,
            ITournamentRunService tournamentRun)
        {
            _overlay = overlay;
            _tournamentRun = tournamentRun;
            _subscription = bus.Subscribe<MatchEndedEvent>(OnMatchEnded);
        }

        public void Dispose() => _subscription?.Dispose();

        private void OnMatchEnded(MatchEndedEvent e)
        {
            _tournamentRun.RecordMatchResult(e.PlayerScore, e.OpponentScore, e.PlayerWon);
            _overlay.SetState(NavigationState.Tournament).Forget();
        }
    }
}
