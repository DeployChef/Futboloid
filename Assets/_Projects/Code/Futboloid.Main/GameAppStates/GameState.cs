using Cysharp.Threading.Tasks;
using Futboloid.Main.DI;
using UnityEngine;
using VContainer.Unity;

namespace Futboloid.Main.GameAppStates
{
    public class GameState
    {
        private readonly LifetimeScope _parentLifetimeScope;

        public LifetimeScope LifetimeScope { get; private set; }

        public GameState(LifetimeScope parentLifetimeScope)
        {
            _parentLifetimeScope = parentLifetimeScope;
        }

        public UniTask Enter()
        {
            LifetimeScope = _parentLifetimeScope.CreateChild(builder => builder.RegisterGameScope());

            Debug.Log("[GameState] Game scope ready, scene views injected.");
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            if (LifetimeScope != null)
            {
                LifetimeScope.Dispose();
                LifetimeScope = null;
            }

            Debug.Log("[GameState] Game scope disposed.");
            return UniTask.CompletedTask;
        }
    }
}
