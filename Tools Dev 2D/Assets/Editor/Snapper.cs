using UnityEditor;
using UnityEngine;
public static class Snapper
{
    const string UNDO_STR_SNAP = "snap objects";

    [MenuItem("Edit/Snap Selected Objects", isValidateFunction:true)]
    public static bool SnapSelectedObjectsValidate()
    {
        return Selection.gameObjects.Length > 0;
    }
    [MenuItem("Edit/Snap Selected Objects")]
    public static void SnapSelectedObjects()
    {
        var selectedObjects = Selection.gameObjects;
        foreach (var selected in selectedObjects)
        {
            Undo.RecordObject(selected.transform, UNDO_STR_SNAP);
            var position = selected.transform.position;
            position = position.Round();
            selected.transform.position = position;
        }
    }

   /*  public static Vector3 Round(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
        return v;
    } */
}


