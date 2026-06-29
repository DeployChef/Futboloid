using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Main.GameAppStates;
using UnityEngine;

namespace Futboloid.Main
{
    public sealed class GameDirector : IGameDirector
    {
        AppRootState appRootState;

        public void InitializeGame()
        {
            RunInitializeAsync().Forget();
        }

        async UniTaskVoid RunInitializeAsync()
        {
            Debug.Log("[GameDirector] Cold start…");

            appRootState = new AppRootState(this);
            await appRootState.Enter();

            Debug.Log("[GameDirector] AppRoot ready. Next: AppGameState + Game scene (этап 2).");
        }

        public void RestartTournament() =>
            Debug.LogWarning("[GameDirector] RestartTournament — not implemented yet.");

        public void RestartMatch() =>
            Debug.LogWarning("[GameDirector] RestartMatch — not implemented yet.");

        public void ReturnToMainMenu() =>
            Debug.LogWarning("[GameDirector] ReturnToMainMenu — not implemented yet.");

        public void SaveGame() =>
            Debug.LogWarning("[GameDirector] SaveGame — not implemented yet.");

        public void LoadLastSave() =>
            Debug.LogWarning("[GameDirector] LoadLastSave — not implemented yet.");
    }
}
