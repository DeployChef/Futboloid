using System;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Bus.Events;
using Futboloid.Main.Navigation;

namespace Futboloid.Main.Navigation
{
    public class MatchEndHandler
    {
        private readonly OverlayStateController _overlay;
        private readonly ITournamentRunService _tournamentRun;

        private IDisposable _subscription;

        public MatchEndHandler(OverlayStateController overlay, ITournamentRunService tournamentRun)
        {
            _overlay = overlay;
            _tournamentRun = tournamentRun;
        }

        public void Bind(IGameEventBus bus)
        {
            Unbind();
            _subscription = bus.Subscribe<MatchEndedEvent>(OnMatchEnded);
        }

        public void Unbind()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnMatchEnded(MatchEndedEvent e)
        {
            _tournamentRun.RecordMatchResult(e.PlayerScore, e.OpponentScore);
            _overlay.SetState(NavigationState.Tournament).Forget();
        }
    }
}
