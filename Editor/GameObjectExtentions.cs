using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public class GameObjectExtentions : MonoBehaviour
{
    static Texture2D texture;
    static Dictionary<int, GameObject> objects;
    //static GUIStyle btnStyle;
    //static GUIStyle boxStyle;

    static GameObjectExtentions()
    {
        texture = Texture2D.whiteTexture;

        

        //btnStyle = new GUIStyle();
        //btnStyle.normal.background = texture;
        //btnStyle.hover.background = texture;
        //btnStyle.active.background = texture;

        EditorApplication.update += UpdateCB;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
    }

    static void UpdateCB()
    {
        // Check here
        //GameObject[] go1 = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        GameObject[] go1 = Resources.FindObjectsOfTypeAll<GameObject>();
        if (objects == null) objects = new Dictionary<int, GameObject>();
        else objects.Clear();

        foreach (GameObject g in go1)
        {
            objects.Add(g.GetInstanceID(), g);
        }
    }

    static void HierarchyItemCB(int instanceID, Rect selectionRect)
    {
        if (objects == null ) return;
        if (objects.ContainsKey(instanceID) && objects[instanceID] != null)
        {
            Rect r = new Rect(selectionRect);
            r.x = r.xMax - 22;
            r.width = 18;
            r.height = 18;
            //r.y += 4;
            //Color col = GUI.color;
            //GUI.color = markedSublines[instanceID].curveColor;
            //GUI.DrawTexture(r, texture);
            bool res = GUI.Toggle(r, objects[instanceID].activeSelf, "");
            if (GUI.changed)
            {
                if(objects[instanceID].activeSelf != res)
                    EditorUtility.SetDirty(objects[instanceID]);
                objects[instanceID].SetActive(res);
            }
            //GUI.color = col;
        }
    }
}
