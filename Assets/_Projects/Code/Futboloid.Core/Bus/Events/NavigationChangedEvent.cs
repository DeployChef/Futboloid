namespace Futboloid.Core.Bus.Events
{
    public readonly struct NavigationChangedEvent
    {
        public NavigationState Previous { get; }
        public NavigationState Current { get; }
        public bool IsMatchPausedInMenu { get; }
        public bool ResumingPausedMatch { get; }

        public NavigationChangedEvent(
            NavigationState previous,
            NavigationState current,
            bool isMatchPausedInMenu = false,
            bool resumingPausedMatch = false)
        {
            Previous = previous;
            Current = current;
            IsMatchPausedInMenu = isMatchPausedInMenu;
            ResumingPausedMatch = resumingPausedMatch;
        }
    }
}
