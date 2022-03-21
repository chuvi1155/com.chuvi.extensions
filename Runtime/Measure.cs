using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Measure : EditorWindow
{
    [MenuItem("Chuvi/Measure")]
    static void Init()
    {
        GetWindow(typeof(Measure), true);
    }

    Transform tr1, tr2;

    // Window has been selected
    void OnFocus()
    {
        // Remove delegate listener if it has previously
        // been assigned.
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        // Add (or re-add) the delegate.
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }

    void OnDestroy()
    {
        // When the window is destroyed, remove the delegate
        // so that it will no longer do any drawing.
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    private void Update()
    {
        Repaint();
    }

    private void OnGUI()
    {
        tr1 = (Transform)EditorGUILayout.ObjectField("Object 1", tr1, typeof(Transform), true);
        tr2 = (Transform)EditorGUILayout.ObjectField("Object 2", tr2, typeof(Transform), true);
        if (tr1 != null && tr2 != null)
        {
            EditorGUILayout.LabelField("Distance: ", Vector3.Distance(tr1.position, tr2.position).ToString());
        }
    }
    void OnSceneGUI(SceneView sceneView)
    {
        if (sceneView.titleContent.text != "Scene") return;
        if (tr1 == null || tr2 == null)
            return;
        Handles.color = Color.blue;
        Handles.DrawLine(tr1.position, tr2.position);
        Handles.color = Color.white;
    }
}