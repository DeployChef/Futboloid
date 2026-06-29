using Futboloid.Core;

namespace Futboloid.Gameplay.Bus.Events
{
    public readonly struct NavigationChangedEvent
    {
        public NavigationState Previous { get; }
        public NavigationState Current { get; }

        public NavigationChangedEvent(NavigationState previous, NavigationState current)
        {
            Previous = previous;
            Current = current;
        }
    }
}
