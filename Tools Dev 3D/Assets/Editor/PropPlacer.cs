using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.EventSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


public class PropPlacer : EditorWindow
{
    [MenuItem("Tools/Prop Placer")]
    public static void OpenPropPlacer() => GetWindow<PropPlacer>("Prop Placer");

    public const float TAU = 6.283185307179586f;
    public float Radius = 2f;
    public int SpawnCount = 8;
    public GameObject ObjectToSpawn;

    private SerializedObject _serializedObject;
    private SerializedProperty _radiusProperty;
    private SerializedProperty _spawnCountProperty;
    private SerializedProperty _objectToSpawnProperty;

    private Vector2[] randomPoints;
    private List<Vector3> pointsOnMesh = new List<Vector3>();


    private const int circleDetail = 64;

    private void OnEnable()
    {
        _serializedObject = new SerializedObject(this);
        _radiusProperty = _serializedObject.FindProperty("Radius");
        _spawnCountProperty = _serializedObject.FindProperty("SpawnCount");
        _objectToSpawnProperty = _serializedObject.FindProperty("ObjectToSpawn");

        SceneView.duringSceneGui += DuringSceneGUI;

        GenerateRandomPoints();
    }


    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void OnGUI()
    {

        EditorGUILayout.LabelField("Configs");

        _serializedObject.Update();
        EditorGUILayout.PropertyField(_radiusProperty);
        _radiusProperty.floatValue = Mathf.Max(0, _radiusProperty.floatValue);
        EditorGUILayout.PropertyField(_spawnCountProperty);
        _spawnCountProperty.intValue = Mathf.Max(0, _spawnCountProperty.intValue);
        EditorGUILayout.PropertyField(_objectToSpawnProperty);


        if (_serializedObject.ApplyModifiedProperties())
        {
            GenerateRandomPoints();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Randomize"))
        {
            GenerateRandomPoints();
            SceneView.RepaintAll();
        }

        if (Event.current.type == EventType.MouseDown)
        {
            GUI.FocusControl(null);
            Repaint();
        }


    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Handles.color = Color.red;

        if (Event.current.type == EventType.MouseMove)
            sceneView.Repaint();

        DrawBrush(sceneView);

        // change radius using scroll wheel    
        bool holdingAlt = (Event.current.modifiers & EventModifiers.Alt) != 0;
        if (Event.current.type == EventType.ScrollWheel && !holdingAlt)
        {
            float scrollDir = Mathf.Sign(Event.current.delta.y);

            _serializedObject.Update();
            _radiusProperty.floatValue *= 1f + -scrollDir * .1f; // scale with its own value
            if (_serializedObject.ApplyModifiedProperties())
            {
                Repaint();
                SceneView.RepaintAll();
            }
            Event.current.Use(); //consume the event
        }

        // place Prefab
        if (Event.current.type == EventType.MouseDown && holdingAlt)
        {
            if (!ObjectToSpawn) return;

            for (int i = 0; i < pointsOnMesh.Count; i++)
            {
                var spawnedObj = (GameObject)PrefabUtility.InstantiatePrefab(ObjectToSpawn);
                spawnedObj.transform.position = pointsOnMesh[i];
            }
            Event.current.Use(); //consume the event
        }

    }

    private void DrawBrush(SceneView sceneView)
    {
        var cameraTransform = sceneView.camera.transform;
        Ray ray = GetMouseRay();

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // setting up tangent space
            Vector3 hitNormal = hit.normal;
            // Get a vector perpanducular to both vectors 
            Vector3 hitTangent = Vector3.Cross(hitNormal, cameraTransform.up).normalized;
            Vector3 hitBitangent = Vector3.Cross(hitNormal, hitTangent);


            Ray getTangentRay(Vector2 tangentSpacePos)
            {
                Vector3 rayOrigin = hit.point + (hitTangent * tangentSpacePos.x + hitBitangent * tangentSpacePos.y) * Radius + hitNormal * 2;
                Vector3 rayDirection = -hitNormal;
                Ray ray = new Ray(rayOrigin, rayDirection);
                return ray;
            }


            // draw circle area
            Handles.DrawAAPolyLine(6, hit.point, hit.point + hit.normal);

            // draw circle adapted to terrain
            Vector3[] circlePoints = new Vector3[circleDetail + 1];
            for (int i = 0; i < circleDetail + 1; i++)
            {
                float t = i / ((float)circleDetail - 1);
                float angRad = t * TAU;
                Vector2 dir = new Vector2(Mathf.Cos(angRad), Mathf.Sin(angRad));
                Ray circlePointRay = getTangentRay(dir);
                if (Physics.Raycast(circlePointRay, out RaycastHit circleHit))
                {
                    circlePoints[i] = circleHit.point;
                }
                else
                {
                    circlePoints[i] = circlePointRay.origin;
                }
            }

            Handles.DrawAAPolyLine(circlePoints);
            //Handles.DrawWireDisc(hit.point, hit.normal, Radius);


            // drawing random points
            pointsOnMesh.Clear();
            foreach (var point in randomPoints)
            {
                Ray pointRay = getTangentRay(point);
                if (Physics.Raycast(pointRay, out RaycastHit pointHit))
                {
                    DrawSphere(pointHit.point);
                    pointsOnMesh.Add(pointHit.point);
                }
            }


        }
    }

    private Ray GetMouseRay()
    {
        return HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
    }

    private void DrawSphere(Vector3 pos)
    {
        Handles.SphereHandleCap(-1, pos, Quaternion.identity, .05f, EventType.Repaint);
    }

    private void GenerateRandomPoints()
    {
        randomPoints = new Vector2[SpawnCount];
        for (int i = 0; i < SpawnCount; i++)
        {
            randomPoints[i] = Random.insideUnitCircle;
        }
    }
}
