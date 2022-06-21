using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public class GameObjectExtentions : MonoBehaviour
{
    static Texture2D texture;
    static Dictionary<int, GameObject> objects;

    static GameObjectExtentions()
    {
        texture = Texture2D.whiteTexture;

        EditorApplication.update += UpdateCB;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
    }

    static void UpdateCB()
    {
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
            bool res = GUI.Toggle(r, objects[instanceID].activeSelf, "");
            if (GUI.changed)
            {
                if (res != objects[instanceID].activeSelf)
                {
                    objects[instanceID].SetActive(res);
                    EditorUtility.SetDirty(objects[instanceID]); 
                }
            }
        }
    }
}
