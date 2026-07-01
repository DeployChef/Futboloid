using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Keeper;
using Futboloid.Gameplay.Match;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>После гола: твины футболистов + мяча, затем Pitch → KickoffWait.</summary>
    public sealed class DefenderReshuffleService : IDisposable
    {
        private readonly PitchStateMachine _pitch;
        private readonly DefenderGridRegistry _registry;
        private readonly BallView _ball;
        private readonly GoalkeeperView _keeper;
        private readonly List<IDisposable> _subscriptions = new();

        private CancellationTokenSource _reshuffleCts;

        public DefenderReshuffleService(
            IGameEventBus bus,
            PitchStateMachine pitch,
            DefenderGridRegistry registry,
            BallView ball,
            GoalkeeperView keeper)
        {
            _pitch = pitch;
            _registry = registry;
            _ball = ball;
            _keeper = keeper;
            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
        }

        public void Dispose()
        {
            CancelReshuffle();
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            if (e.Phase != PitchPhase.Reshuffle)
                return;

            RunReshuffleAsync().Forget();
        }

        private async UniTaskVoid RunReshuffleAsync()
        {
            CancelReshuffle();
            _reshuffleCts = new CancellationTokenSource();
            var ct = _reshuffleCts.Token;

            try
            {
                var tasks = new List<UniTask>(8);
                if (_ball != null)
                    tasks.Add(_ball.PlayReshuffleToKickoffAsync(ct));

                var moveDuration = _registry != null ? _registry.ReshuffleMoveDuration : 0.55f;
                if (_keeper != null)
                    tasks.Add(_keeper.PlayReshuffleToCenterAsync(moveDuration, ct));

                if (_registry != null)
                {
                    _registry.ForEachLiving(defender =>
                        tasks.Add(defender.PlayReshuffleTweenAsync(
                            _registry.ReshuffleMoveDuration,
                            _registry.ArriveThreshold,
                            ct)));
                }

                if (tasks.Count > 0)
                    await UniTask.WhenAll(tasks);

                if (ct.IsCancellationRequested || _pitch.Current != PitchPhase.Reshuffle)
                    return;

                _pitch.CompleteReshuffle();
            }
            catch (OperationCanceledException)
            {
                // Новый решафл или выгрузка сцены.
            }
        }

        private void CancelReshuffle()
        {
            if (_reshuffleCts == null)
                return;

            _reshuffleCts.Cancel();
            _reshuffleCts.Dispose();
            _reshuffleCts = null;
        }
    }
}
