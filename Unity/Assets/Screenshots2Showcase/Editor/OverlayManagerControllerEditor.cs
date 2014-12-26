using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OverlayManagerController))]
public class OverlayManagerControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = target as OverlayManagerController;

        if (GUILayout.Button("Previous"))
        {
            t.visibleChildIndex--;
            t.UpdateOverlays();
        }

        if (GUILayout.Button("Next"))
        {
            t.visibleChildIndex++;
            t.UpdateOverlays();
        }

        if (GUILayout.Button("Add Empty Overlay"))
        {
            var overlay = new GameObject();
            overlay.AddComponent<OverlayController>();
            overlay.name = "Overlay";
            overlay.transform.parent = t.transform;

            Undo.IncrementCurrentGroup();
            Undo.RegisterCreatedObjectUndo(overlay, "Created overlay");
            Undo.RecordObject(t, "Change Child Index");
            t.visibleChildIndex = t.transform.childCount - 1;
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        if (t.transform.childCount > 0)
        {
            t.ValidateChildIndex();

            var current = t.transform.GetChild(t.visibleChildIndex).gameObject;

            if (GUILayout.Button("Duplicate Overlay"))
            {
                var overlay = (GameObject)Instantiate(current);
                overlay.name = current.name;
                overlay.transform.parent = t.transform;

                Undo.IncrementCurrentGroup();
                Undo.RegisterCreatedObjectUndo(overlay, "Duplicated overlay");
                Undo.RecordObject(t, "Change Child Index");
                t.visibleChildIndex = t.transform.childCount - 1;
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
        }
    }
}
