using Cysharp.Threading.Tasks;
using Futboloid.Main.DI;
using UnityEngine.SceneManagement;
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

        public UniTask Enter(Scene gameScene)
        {
            LifetimeScope = _parentLifetimeScope.CreateChild(
                builder => builder.RegisterGameScope(gameScene));

            UnityEngine.Debug.Log("[GameState] Game scope ready, scene views injected.");
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            if (LifetimeScope != null)
            {
                LifetimeScope.Dispose();
                LifetimeScope = null;
            }

            UnityEngine.Debug.Log("[GameState] Game scope released.");
            return UniTask.CompletedTask;
        }
    }
}
