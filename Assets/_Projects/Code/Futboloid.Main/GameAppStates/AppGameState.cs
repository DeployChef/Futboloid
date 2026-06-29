using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Main.DI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Futboloid.Main.GameAppStates
{
  public sealed class AppGameState
  {
    readonly LifetimeScope parentLifetimeScope;

    public LifetimeScope LifetimeScope { get; private set; }

    public AppGameState(LifetimeScope parentLifetimeScope)
    {
      this.parentLifetimeScope = parentLifetimeScope;
    }

    public async UniTask Enter()
    {
      LifetimeScope = parentLifetimeScope.CreateChild(builder => builder.RegisterAppScope());

      await SceneManager.LoadSceneAsync(GameScenes.Game, LoadSceneMode.Additive).ToUniTask();

      var gameScene = SceneManager.GetSceneByName(GameScenes.Game);
      if (!gameScene.IsValid())
      {
        Debug.LogError($"[AppGameState] Scene '{GameScenes.Game}' not found after load. Is it in Build Settings?");
        return;
      }

      SceneManager.SetActiveScene(gameScene);

      Debug.Log($"[AppGameState] '{GameScenes.Game}' loaded additive, active scene set.");
    }

    public async UniTask Exit()
    {
      if (LifetimeScope != null)
      {
        LifetimeScope.Dispose();
        LifetimeScope = null;
      }

      var gameScene = SceneManager.GetSceneByName(GameScenes.Game);
      if (gameScene.IsValid())
        await SceneManager.UnloadSceneAsync(gameScene).ToUniTask();

      Debug.Log($"[AppGameState] '{GameScenes.Game}' unloaded.");
    }
  }
}
