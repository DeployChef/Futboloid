using Futboloid.Gameplay.Bus;
using Futboloid.Gameplay.Bus.Events;
using UnityEngine;

namespace Futboloid.Gameplay.Match
{
    public class MatchFlow
    {
        private readonly IGameEventBus _bus;

        public int PlayerScore { get; private set; }
        public int OpponentScore { get; private set; }

        public MatchFlow(IGameEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<GoalScoredEvent>(OnGoalScored);
        }

        public void Reset()
        {
            PlayerScore = 0;
            OpponentScore = 0;
        }

        public void RecordGoal(bool isPlayerGoal)
        {
            if (isPlayerGoal)
                PlayerScore++;
            else
                OpponentScore++;

            Debug.Log($"[MatchFlow] Score {PlayerScore}:{OpponentScore} (player goal={isPlayerGoal})");
        }

        private void OnGoalScored(GoalScoredEvent e)
        {
            RecordGoal(e.IsPlayerGoal);
        }
    }
}
