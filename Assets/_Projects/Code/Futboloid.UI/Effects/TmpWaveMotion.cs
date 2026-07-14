using TMPro;
using UnityEngine;

namespace Futboloid.UI.Effects
{
    /// <summary>
    /// Волна по буквам TextMeshPro. Повесь на объект с TMP_Text / TextMeshProUGUI.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpWaveMotion : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float waveAmplitude = 6f;
        [SerializeField] private float waveSpeed = 3f;
        [SerializeField] private float waveSpacing = 0.45f;
        [SerializeField] private bool useUnscaledTime;

        private void Reset() => text = GetComponent<TMP_Text>();

        private void Awake()
        {
            if (text == null)
                text = GetComponent<TMP_Text>();
        }

        private void LateUpdate()
        {
            if (text == null || !isActiveAndEnabled)
                return;

            AnimateWave();
        }

        private void AnimateWave()
        {
            text.ForceMeshUpdate();
            var textInfo = text.textInfo;
            if (textInfo.characterCount == 0)
                return;

            var time = useUnscaledTime ? Time.unscaledTime : Time.time;

            for (var i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                var materialIndex = charInfo.materialReferenceIndex;
                var vertexIndex = charInfo.vertexIndex;
                var vertices = textInfo.meshInfo[materialIndex].vertices;
                var wave = Mathf.Sin(time * waveSpeed + i * waveSpacing) * waveAmplitude;

                vertices[vertexIndex + 0].y += wave;
                vertices[vertexIndex + 1].y += wave;
                vertices[vertexIndex + 2].y += wave;
                vertices[vertexIndex + 3].y += wave;

                textInfo.meshInfo[materialIndex].vertices = vertices;
            }

            for (var i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                text.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}
