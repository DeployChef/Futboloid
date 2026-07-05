using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Run;
using Futboloid.Gameplay.Match;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Реестр живых футболистов на сцене (коллайдеры, вайп, поиск для паса).</summary>
    public class DefenderGridRegistry : MonoBehaviour
    {
        private readonly Dictionary<EntityId, DefenderView> _byColliderId = new();
        private readonly List<DefenderView> _defenders = new();

        private IGameEventBus _bus;
        private MatchFlow _matchFlow;
        private IRunProgressionService _run;
        private CancellationTokenSource _wipeCheckCts;

        [Inject]
        public void Construct(IGameEventBus bus, MatchFlow matchFlow, IRunProgressionService run)
        {
            _bus = bus;
            _matchFlow = matchFlow;
            _run = run;
            _bus.Subscribe<DefenderDestroyedEvent>(OnDefenderDestroyed);
        }

        private void OnDestroy()
        {
            _wipeCheckCts?.Cancel();
            _wipeCheckCts?.Dispose();
        }

        public void Register(DefenderView defender)
        {
            if (defender == null || _defenders.Contains(defender))
                return;

            _defenders.Add(defender);
            RegisterCollider(defender.ContactCollider, defender);
        }

        public void Unregister(DefenderView defender)
        {
            if (defender == null)
                return;

            UnregisterCollider(defender.ContactCollider);
            _defenders.Remove(defender);
        }

        public bool TryGetDefender(Collider2D contactCollider, out DefenderView defender)
        {
            defender = null;
            if (contactCollider == null)
                return false;

            return _byColliderId.TryGetValue(contactCollider.GetEntityId(), out defender);
        }

        private void RegisterCollider(Collider2D contactCollider, DefenderView defender)
        {
            if (contactCollider == null || defender == null)
                return;

            _byColliderId[contactCollider.GetEntityId()] = defender;
        }

        private void UnregisterCollider(Collider2D contactCollider)
        {
            if (contactCollider == null)
                return;

            _byColliderId.Remove(contactCollider.GetEntityId());
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

        public bool HasLivingGoalkeeper()
        {
            for (var i = 0; i < _defenders.Count; i++)
            {
                var defender = _defenders[i];
                if (defender != null && defender.IsAlive && defender.Role == DefenderRole.Goalkeeper)
                    return true;
            }

            return false;
        }

        public DefenderView FindNearestLivingField(Vector2 point, bool excludeRunning = false)
        {
            DefenderView best = null;
            var bestDist = float.MaxValue;

            for (var i = 0; i < _defenders.Count; i++)
            {
                var defender = _defenders[i];
                if (defender == null || !defender.IsAlive || defender.Role != DefenderRole.Field)
                    continue;

                if (excludeRunning && defender.RunningToGoal)
                    continue;

                var dist = ((Vector2)defender.transform.position - point).sqrMagnitude;
                if (dist >= bestDist)
                    continue;

                bestDist = dist;
                best = defender;
            }

            return best;
        }

        public void ForEachLiving(Action<DefenderView> action)
        {
            if (action == null)
                return;

            for (var i = 0; i < _defenders.Count; i++)
            {
                var defender = _defenders[i];
                if (defender != null && defender.IsAlive)
                    action(defender);
            }
        }

        private void OnDefenderDestroyed(DefenderDestroyedEvent _)
        {
            if (_matchFlow == null || AliveCount > 0)
                return;

            _wipeCheckCts?.Cancel();
            _wipeCheckCts?.Dispose();
            _wipeCheckCts = new CancellationTokenSource();
            ScheduleWipeVictoryCheckAsync(_wipeCheckCts.Token).Forget();
        }

        private async UniTaskVoid ScheduleWipeVictoryCheckAsync(CancellationToken ct)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct);

            if (ct.IsCancellationRequested || _matchFlow == null || AliveCount > 0)
                return;

            _matchFlow.MarkWipeVictoryPending();
            _matchFlow.TryCompleteWipeVictory(_run);
        }
    }
}
