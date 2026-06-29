using Futboloid.Gameplay.Bus;

namespace Futboloid.Gameplay.Match
{
    /// <summary>
    /// Таймер, счёт, фазы матча — логика позже.
    /// </summary>
    public class MatchFlow
    {
        private readonly IGameEventBus _bus;

        public MatchFlow(IGameEventBus bus)
        {
            _bus = bus;
        }

        public void Reset()
        {
            // TODO: сброс таймера и счёта
        }
    }
}
