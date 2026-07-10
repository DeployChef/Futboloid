using UnityEngine;

namespace Futboloid.Gameplay.Characters
{
    /// <summary>
    /// Idle / Run через bool Run на Animator. Поворот спрайта по знаку горизонтальной скорости.
    /// </summary>
    public sealed class CharacterAnimationPresenter : MonoBehaviour
    {
        private const float FaceVelocityThreshold = 0.001f;

        private static readonly int RunHash = Animator.StringToHash("Run");

        [SerializeField] private Animator animator;
        [SerializeField] private Transform facingRoot;
        [SerializeField] private Transform numbersFace;

        private float _scaleXAbs = 1f;
        private float _lastFaceSign = 1f;

        private void Awake()
        {
            var root = FacingRoot;
            _scaleXAbs = Mathf.Abs(root.localScale.x);
            if (_scaleXAbs < 0.0001f)
                _scaleXAbs = 1f;

            _lastFaceSign = root.localScale.x >= 0f ? 1f : -1f;
        }

        private Transform FacingRoot => facingRoot != null ? facingRoot : transform;

        public void SetLocomotion(bool isRunning, float velocityX = 0f)
        {
            if (animator != null)
                animator.SetBool(RunHash, isRunning);

            if (Mathf.Abs(velocityX) > FaceVelocityThreshold)
                _lastFaceSign = velocityX > 0f ? -1f : 1f;

            ApplyFacing();
        }

        private void ApplyFacing()
        {
            var root = FacingRoot;
            var scale = root.localScale;
            scale.x = _scaleXAbs * _lastFaceSign;
            root.localScale = scale;

            if (numbersFace)
            {
                var scaleNumX = Mathf.Abs(numbersFace.localScale.x);
                var scaleNum = numbersFace.localScale;
                scaleNum.x = scaleNumX * _lastFaceSign;
                numbersFace.localScale = scaleNum;
            }
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
