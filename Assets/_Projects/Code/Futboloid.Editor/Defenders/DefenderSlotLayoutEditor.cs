#if UNITY_EDITOR
using Futboloid.Gameplay.Defenders;
using UnityEditor;
using UnityEngine;

namespace Futboloid.Editor.Defenders
{
    [CustomEditor(typeof(DefenderSlotLayout))]
    public sealed class DefenderSlotLayoutEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");

            var layout = (DefenderSlotLayout)target;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Slot grid", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate grid"))
                    GenerateGrid(layout);

                if (GUILayout.Button("Clear slots"))
                    ClearSlots(layout);
            }

            EditorGUILayout.HelpBox(
                "Generate grid: создаёт пустые дочерние объекты Slot_0, Slot_1… и заполняет Slot Points.\n" +
                "Сетка центрируется на позиции DefenderSlots. Верхний ряд — выше по Y.",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateGrid(DefenderSlotLayout layout)
        {
            var columns = Mathf.Max(1, layout.GridColumns);
            var rows = Mathf.Max(1, layout.GridRows);
            var spacingX = Mathf.Max(0.1f, layout.GridSpacingX);
            var spacingY = Mathf.Max(0.1f, layout.GridSpacingY);
            var count = columns * rows;

            ClearSlots(layout, recordUndo: false);

            var slotPoints = new Transform[count];
            var center = layout.transform.position;
            var startX = center.x - (columns - 1) * spacingX * 0.5f;
            var startY = center.y + (rows - 1) * spacingY * 0.5f;
            var index = 0;

            Undo.RegisterFullObjectHierarchyUndo(layout.gameObject, "Generate defender slot grid");

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    var slotObject = new GameObject($"Slot_{index}");
                    Undo.RegisterCreatedObjectUndo(slotObject, "Generate defender slot grid");

                    var slotTransform = slotObject.transform;
                    slotTransform.SetParent(layout.transform, worldPositionStays: false);
                    slotTransform.position = new Vector3(
                        startX + col * spacingX,
                        startY - row * spacingY,
                        center.z);

                    slotPoints[index] = slotTransform;
                    index++;
                }
            }

            layout.EditorSetSlotPoints(slotPoints);
            EditorUtility.SetDirty(layout);
        }

        private static void ClearSlots(DefenderSlotLayout layout, bool recordUndo = true)
        {
            if (recordUndo)
                Undo.RegisterFullObjectHierarchyUndo(layout.gameObject, "Clear defender slots");

            var existing = layout.SlotPoints;
            if (existing != null)
            {
                for (var i = 0; i < existing.Count; i++)
                {
                    var point = existing[i];
                    if (point == null)
                        continue;

                    Undo.DestroyObjectImmediate(point.gameObject);
                }
            }

            for (var i = layout.transform.childCount - 1; i >= 0; i--)
            {
                var child = layout.transform.GetChild(i);
                if (!child.name.StartsWith("Slot_"))
                    continue;

                Undo.DestroyObjectImmediate(child.gameObject);
            }

            layout.EditorSetSlotPoints(System.Array.Empty<Transform>());
            EditorUtility.SetDirty(layout);
        }
    }
}
#endif
