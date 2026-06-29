using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Match;

namespace Futboloid.Main.Session
{
    /// <summary>
    /// Мост App scope ↔ Game scope. App-сервисы берут pitch/bus отсюда после загрузки Game.
    /// </summary>
    public class GameSession
    {
        public IGameEventBus Bus { get; private set; }
        public MatchFlow MatchFlow { get; private set; }
        public PitchStateMachine Pitch { get; private set; }
        public bool IsGameBound => Pitch != null;

        public void BindGameScope(IGameEventBus bus, MatchFlow matchFlow, PitchStateMachine pitch)
        {
            Bus = bus;
            MatchFlow = matchFlow;
            Pitch = pitch;
        }

        public void ClearGameScope()
        {
            Bus = null;
            MatchFlow = null;
            Pitch = null;
        }
    }
}
