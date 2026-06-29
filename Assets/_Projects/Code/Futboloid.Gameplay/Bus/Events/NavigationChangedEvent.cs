using Futboloid.Core;

namespace Futboloid.Gameplay.Bus.Events
{
    public readonly struct NavigationChangedEvent
    {
        public NavigationState Previous { get; }
        public NavigationState Current { get; }
        public bool IsMatchPausedInMenu { get; }

        public NavigationChangedEvent(
            NavigationState previous,
            NavigationState current,
            bool isMatchPausedInMenu = false)
        {
            Previous = previous;
            Current = current;
            IsMatchPausedInMenu = isMatchPausedInMenu;
        }
    }
}
