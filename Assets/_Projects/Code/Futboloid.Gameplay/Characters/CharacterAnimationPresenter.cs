using UnityEngine;

namespace Futboloid.Gameplay.Characters
{
    /// <summary>
    /// Idle / Run через bool Run на Animator. Animator — только из Inspector.
    /// </summary>
    public sealed class CharacterAnimationPresenter : MonoBehaviour
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
                Debug.LogWarning("[CharacterAnimationPresenter] Animator is not assigned.", this);
        }
#endif
    }
}
