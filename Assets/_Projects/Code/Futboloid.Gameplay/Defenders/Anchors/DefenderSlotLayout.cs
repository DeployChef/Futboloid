using System.Collections.Generic;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>
    /// Реестр точек расстановки на сцене. Перетащи пустые GameObject в массив Slot Points.
    /// Slot Id = индекс в массиве (0, 1, 2…). Источник позиций для генерации врагов.
    /// </summary>
    public class DefenderSlotLayout : MonoBehaviour
    {
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private float gizmoRadius = 0.22f;
        [SerializeField] private Transform[] slotPoints = System.Array.Empty<Transform>();

        [Header("Grid generation (editor)")]
        [SerializeField] private int gridColumns = 5;
        [SerializeField] private int gridRows = 4;
        [SerializeField] private float gridSpacingX = 1.15f;
        [SerializeField] private float gridSpacingY = 0.95f;

        public IReadOnlyList<Transform> SlotPoints => slotPoints;

        public int SlotCount => slotPoints != null ? slotPoints.Length : 0;

        public bool TryGetPosition(int slotId, out Vector2 position)
        {
            position = default;
            if (slotPoints == null || slotId < 0 || slotId >= slotPoints.Length)
                return false;

            var point = slotPoints[slotId];
            if (point == null)
                return false;

            position = point.position;
            return true;
        }

#if UNITY_EDITOR
        public int GridColumns => gridColumns;
        public int GridRows => gridRows;
        public float GridSpacingX => gridSpacingX;
        public float GridSpacingY => gridSpacingY;

        public void EditorSetSlotPoints(Transform[] points)
        {
            slotPoints = points ?? System.Array.Empty<Transform>();
        }

        private void OnValidate() => ValidateSlotPoints();

        private void ValidateSlotPoints()
        {
            if (slotPoints == null)
                return;

            for (var i = 0; i < slotPoints.Length; i++)
            {
                if (slotPoints[i] == null)
                {
                    Debug.LogWarning(
                        $"[DefenderSlotLayout] Slot Points[{i}] is empty on '{name}'.",
                        this);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            DrawSlotGizmos(selected: false);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            DrawSlotGizmos(selected: true);

            if (slotPoints != null && slotPoints.Length > 0)
            {
                var labelPos = transform.position;
                DefenderGizmoDrawer.DrawLabel(
                    new Vector3(labelPos.x, labelPos.y, labelPos.z),
                    $"Slots: {slotPoints.Length}");
            }
        }

        private void DrawSlotGizmos(bool selected)
        {
            if (slotPoints == null)
                return;

            var color = selected
                ? new Color(0.35f, 1f, 0.55f, 0.95f)
                : new Color(0.35f, 1f, 0.55f, 0.45f);

            for (var i = 0; i < slotPoints.Length; i++)
            {
                var point = slotPoints[i];
                if (point == null)
                    continue;

                var center = point.position;
                var z = center.z;

                DefenderGizmoDrawer.DrawWireCircle(center, gizmoRadius, color);
                DefenderGizmoDrawer.DrawLabel(
                    new Vector3(center.x, center.y + gizmoRadius + 0.08f, z),
                    $"#{i}");
            }
        }
#endif
    }
}
