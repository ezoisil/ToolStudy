using System;
using TMPro;
using UnityEditor;
using UnityEngine;

public class SnapperTool : EditorWindow
{
    public enum GridTypes
    {
        Cartesian,
        Polar,
    }
    const string UNDO_STR_SNAP = "snap objects";

    public GridTypes GridType = GridTypes.Cartesian;
    public float GridSize = 1;
    public float GridDrawExtent = 10;
    public int AngularDivision = 24;

    private const float TAU = 6.2831855f;
    private SerializedObject _serializedObject;
    private SerializedProperty _gridSizeProperty;
    private SerializedProperty _gridDrawExtentProperty;
    private SerializedProperty _gridTypeProperty;
    private SerializedProperty _angularDivisionProperty;

    [MenuItem("Tools/Snapper")]
    public static void OpenTheTool() => GetWindow<SnapperTool>("Snapper");

    private void OnEnable()
    {
        _serializedObject = new SerializedObject(this);
        _gridSizeProperty = _serializedObject.FindProperty("GridSize");
        _gridDrawExtentProperty = _serializedObject.FindProperty("GridDrawExtent");
        _gridTypeProperty = _serializedObject.FindProperty("GridType");
        _angularDivisionProperty = _serializedObject.FindProperty("AngularDivision");

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGui;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGui;
    }
    private void OnGUI()
    {
        _serializedObject.Update();
        EditorGUILayout.PropertyField(_gridTypeProperty);
        EditorGUILayout.PropertyField(_gridSizeProperty);
        EditorGUILayout.PropertyField(_gridDrawExtentProperty);

        if (GridType == GridTypes.Polar)
        {
            EditorGUILayout.PropertyField(_angularDivisionProperty);
            _angularDivisionProperty.intValue = Mathf.Max(4, _angularDivisionProperty.intValue);
        }
        _serializedObject.ApplyModifiedProperties();


        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Snap Selection", GUILayout.MaxWidth(100)))
            {
                SnapSelectedGameObjects();
            }
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

        }
    }
    private void DuringSceneGui(SceneView view)
    {
        if (Event.current.type != EventType.Repaint) return;

        switch (GridType)
        {
            case GridTypes.Cartesian:
                DrawCartesianGrid();
                break;
            case GridTypes.Polar:
                DrawPolarGrid();
                break;
        }
    }

    private void DrawCartesianGrid()
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        int lineCount = Mathf.RoundToInt(GridDrawExtent * 2 / GridSize);
        int halfLineCount = lineCount / 2;

        for (int i = -halfLineCount; i < halfLineCount + 1; i++)
        {
            var xCoord = i * GridSize;
            var yCoord0 = halfLineCount * GridSize;
            var yCoord1 = -halfLineCount * GridSize;
            Vector2 p0 = new Vector2(xCoord, yCoord0);
            Vector2 p1 = new Vector2(xCoord, yCoord1);

            Handles.color = i == 0 ? Color.white : Color.gray;
            Handles.DrawAAPolyLine(p0, p1);
        }

        for (int i = -halfLineCount; i < halfLineCount + 1; i++)
        {
            var yCoord = i * GridSize;
            var xCoord0 = halfLineCount * GridSize;
            var xCoord1 = -halfLineCount * GridSize;
            Vector2 p0 = new Vector2(xCoord0, yCoord);
            Vector2 p1 = new Vector2(xCoord1, yCoord);

            Handles.color = i == 0 ? Color.white : Color.gray;
            Handles.DrawAAPolyLine(p0, p1);
        }
    }

    private void DrawPolarGrid()
    {
        Handles.color = Color.grey;
        int lineCount = Mathf.RoundToInt(GridDrawExtent / GridSize);

        for (int i = 1; i <= lineCount; i++)
        {
            Handles.DrawWireDisc(Vector3.zero, Vector3.forward, GridSize * i);
        }

        for (int i = 0; i < AngularDivision; i++)
        {
            var t = i / (float)AngularDivision;
            float xCoord = Mathf.Cos(TAU * t);
            float yCoord = Mathf.Sin(TAU * t);
            Vector3 pos = new Vector3(xCoord, yCoord, 0) * GridSize * lineCount;
            Handles.DrawAAPolyLine(Vector3.zero, pos);
        }
    }
    private void SnapSelectedGameObjects()
    {
        var selectedObjects = Selection.gameObjects;

        foreach (var selected in selectedObjects)
        {
            Undo.RecordObject(selected.transform, UNDO_STR_SNAP);
            var position = selected.transform.position;
            position = GetSnappedPosition(position);
            selected.transform.position = position;
        }

    }

    private Vector2 GetSnappedPosition(Vector3 originalPos)
    {
        if (GridType == GridTypes.Cartesian)
            return originalPos.Round(GridSize);

        if (GridType == GridTypes.Polar)
        {
            Vector2 position = new Vector2(originalPos.x, originalPos.y);
            float distance = position.magnitude;

            float snappedDistance = distance.Round(GridSize);

            float angleRad = Mathf.Atan2(position.y, position.x); // 0 to TAU
            float angTurns = angleRad / TAU; // 0 to 1
            float angRadSnapped = angTurns.Round( 1f / AngularDivision) * TAU;

            Vector3 snappedDir = new Vector3(
                Mathf.Cos(angRadSnapped),
                Mathf.Sin(angRadSnapped),
                0);

            Vector3 snappedVector = snappedDir * snappedDistance;
            return snappedVector;
        }

        return originalPos;

    }

}
