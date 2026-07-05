using UnityEngine;

namespace Futboloid.Gameplay.Keeper
{
    /// <summary>
    /// Только Animator-параметры вратаря. Animator назначается в Inspector — без GetComponent.
    /// </summary>
    public sealed class GoalkeeperAnimationPresenter : MonoBehaviour
    {
        private static readonly int RunHash = Animator.StringToHash("Run");

        [SerializeField] private Animator animator;

        public void SetRunning(bool isRunning)
        {
            if (animator == null)
                return;

            animator.SetBool(RunHash, isRunning);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (animator == null)
                Debug.LogWarning("[GoalkeeperAnimationPresenter] Animator is not assigned.", this);
        }
#endif
    }
}
