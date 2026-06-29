namespace Futboloid.Gameplay.Bus.Events
{
    public readonly struct GoalScoredEvent
    {
        public bool IsPlayerGoal { get; }

        public GoalScoredEvent(bool isPlayerGoal)
        {
            IsPlayerGoal = isPlayerGoal;
        }
    }
}
