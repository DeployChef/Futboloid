using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Main.DI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Futboloid.Main.GameAppStates
{
    public sealed class AppRootState
    {
        readonly IGameDirector gameDirector;

        public LifetimeScope RootLifetimeScope { get; private set; }

        public AppRootState(IGameDirector gameDirector)
        {
            this.gameDirector = gameDirector;
        }

        public async UniTask Enter()
        {
            var rootScene = SceneManager.CreateScene("RootScene");
            SceneManager.SetActiveScene(rootScene);

            RootLifetimeScope = LifetimeScope.Create(builder => builder.RegisterRootScope(gameDirector));
            SceneManager.MoveGameObjectToScene(RootLifetimeScope.gameObject, rootScene);

            Debug.Log("[AppRootState] RootScene + Root LifetimeScope created.");

            await UniTask.CompletedTask;
        }
    }
}
