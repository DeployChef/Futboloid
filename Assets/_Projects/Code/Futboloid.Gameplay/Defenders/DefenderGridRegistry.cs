using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Match;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>
    /// Список футболистов матча: вайп, позже — пас к ближайшему.
    /// </summary>
    public class DefenderGridRegistry : MonoBehaviour
    {
        private readonly List<DefenderView> _defenders = new();

        private IGameEventBus _bus;
        private MatchFlow _matchFlow;

        [Inject]
        public void Construct(IGameEventBus bus, MatchFlow matchFlow)
        {
            _bus = bus;
            _matchFlow = matchFlow;
            _bus.Subscribe<DefenderDestroyedEvent>(OnDefenderDestroyed);
        }

        public void Register(DefenderView defender)
        {
            if (defender == null || _defenders.Contains(defender))
                return;

            _defenders.Add(defender);
        }

        public void Unregister(DefenderView defender)
        {
            if (defender == null)
                return;

            _defenders.Remove(defender);
        }

        public int AliveCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _defenders.Count; i++)
                {
                    if (_defenders[i] != null && _defenders[i].IsAlive)
                        count++;
                }

                return count;
            }
        }

        private void OnDefenderDestroyed(DefenderDestroyedEvent _)
        {
            if (_matchFlow == null || AliveCount > 0)
                return;

            _matchFlow.EndMatchFromWipe();
        }
    }
}
