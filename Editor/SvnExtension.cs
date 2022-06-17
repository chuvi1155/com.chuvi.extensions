using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SvnExtension : EditorWindow
{
    #region ContextMenu
    private const string svn_AddPathMenu = "SVN/Commands/Add";
    private const string svn_CommitPathMenu = "SVN/Commands/Commit";
    private const string svn_UpdatePathMenu = "SVN/Commands/Update";
    private const string svn_RevertPathMenu = "SVN/Commands/Revert";
    private const string svn_BrowserPathMenu = "SVN/Commands/Browser";

    #region add
    [MenuItem(svn_AddPathMenu, false, 11)]
    static void OnAddClick()
    {
        //Debug.Log(Directory.GetCurrentDirectory());
        var rootPath = Directory.GetCurrentDirectory();
        System.Diagnostics.Process.Start("C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe", $"/command:add /path:\"{rootPath}\"");
    }
    [MenuItem(svn_AddPathMenu, true)]
    static bool CanUseAdd()
    {
        return CanUseSVNCheck();
    }
    #endregion

    #region commit
    [MenuItem(svn_CommitPathMenu, false, 12)]
    static void OnCommitClick()
    {
        //Debug.Log(Directory.GetCurrentDirectory());
        var rootPath = Directory.GetCurrentDirectory();
        System.Diagnostics.Process.Start("C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe", $"/command:commit /path:\"{rootPath}\"");
    }
    [MenuItem(svn_CommitPathMenu, true)]
    static bool CanUseCommit()
    {
        return CanUseSVNCheck();
    }
    void OnCommitSelectedFileClick()
    {
#if UNITY_2019
        if (Selection.objects.Length == 0) return;
#elif UNITY_2020_1_OR_NEWER
        if (Selection.count == 0) return; 
#endif
        List<string> files = new List<string>();
#if UNITY_2019
        for (int i = 0; i < Selection.objects.Length; i++)
#elif UNITY_2020_1_OR_NEWER
        for (int i = 0; i < Selection.count; i++) 
#endif
        {
            string path = AssetDatabase.GetAssetPath(Selection.objects[i]);
            Debug.Log(path);
            if (File.Exists(path))
            {
                files.Add(new FileInfo(path).FullName);
                files.Add(new FileInfo(path).FullName + ".meta");
            }

        }
        if (files.Count > 0)
            System.Diagnostics.Process.Start("C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe", $"/command:commit /path:\"{string.Join("*", files)}\"");
    }
    #endregion

    #region update
    [MenuItem(svn_UpdatePathMenu, false, 13)]
    static void OnUpdateClick()
    {
        //Debug.Log(Directory.GetCurrentDirectory());
        var rootPath = Directory.GetCurrentDirectory();
        System.Diagnostics.Process.Start("C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe", $"/command:update /path:\"{rootPath}\"");
    }
    [MenuItem(svn_UpdatePathMenu, true)]
    static bool CanUseUpdate()
    {
        return CanUseSVNCheck();
    }
    #endregion

    #region revert
    [MenuItem(svn_RevertPathMenu, false, 24)]
    static void OnRevertClick()
    {
        //Debug.Log(Directory.GetCurrentDirectory());
        var rootPath = Directory.GetCurrentDirectory();
        System.Diagnostics.Process.Start("C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe", $"/command:revert /path:\"{rootPath}\"");
    }
    [MenuItem(svn_RevertPathMenu, true)]
    static bool CanUseRevert()
    {
        return CanUseSVNCheck();
    }
    void OnRevertSelectedFileClick()
    {
#if UNITY_2019
        if (Selection.objects.Length == 0) return;
#elif UNITY_2020_1_OR_NEWER
        if (Selection.count == 0) return; 
#endif
        List<string> files = new List<string>();
#if UNITY_2019
        for (int i = 0; i < Selection.objects.Length; i++)
#elif UNITY_2020_1_OR_NEWER
        for (int i = 0; i < Selection.count; i++) 
#endif
        {
            string path = AssetDatabase.GetAssetPath(Selection.objects[i]);
            Debug.Log(path);
            if (File.Exists(path))
            {
                files.Add(new FileInfo(path).FullName);
                files.Add(new FileInfo(path).FullName + ".meta");
            }
                
        }
        if(files.Count > 0)
            System.Diagnostics.Process.Start("C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe", $"/command:revert /path:\"{string.Join("*", files)}\"");
    }
    #endregion

    #region browser
    [MenuItem(svn_BrowserPathMenu, false, 25)]
    static void OnBrowserClick()
    {
        //Debug.Log(Directory.GetCurrentDirectory());
        var rootPath = Directory.GetCurrentDirectory();
        System.Diagnostics.Process.Start("C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe", $"/command:repobrowser /path:\"{rootPath}\"");
    }
    [MenuItem(svn_BrowserPathMenu, true)]
    static bool CanUseBrowser()
    {
        return CanUseSVNCheck();
    }
    #endregion


    static bool CanUseSVNCheck()
    {
        return File.Exists("C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe");// && CanUseSVNWork();
    }
    static bool CanUseSVNWork()
    {
        var rootPath = Directory.GetCurrentDirectory();
        return Directory.Exists(Path.Combine(rootPath, ".svn"));
    }
    #endregion

    #region window
    [MenuItem("SVN/GUI Window", false, 0)]
    static void OnShowUI()
    {
        GetWindow<SvnExtension>(true, "SVN");
    }
    #endregion

    bool SomeSelectedFileExist()
    {
#if UNITY_2019
        if (Selection.objects.Length == 0) return false;
#elif UNITY_2020_1_OR_NEWER
        if (Selection.count == 0) return false; 
#endif
        {
#if UNITY_2019
            for (int i = 0; i < Selection.objects.Length; i++)
#elif UNITY_2020_1_OR_NEWER
            for (int i = 0; i < Selection.count; i++) 
#endif
            {
                if (File.Exists(AssetDatabase.GetAssetPath(Selection.objects[i])))
                    return true;
            }
        }
        return false;
    }
    private void OnGUI()
    {
        if (!CanUseSVNCheck())
        {
            EditorGUILayout.HelpBox("Скачайте приложение TortoiseSVN и установите в директорию по умолчанию 'C:\\Program Files\\TortoiseSVN'", MessageType.Error);
            return;
        }
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            GUI.enabled = false;
            EditorGUILayout.HelpBox("Сохраните сцену перед ее выгрузкой в SVN", MessageType.Warning);
        }
        else GUI.enabled = true;
        if (GUILayout.Button("Add"))
            OnAddClick();
        if (GUILayout.Button("Commit"))
            OnCommitClick();
        if (GUILayout.Button("Update"))
            OnUpdateClick();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Revert all"))
            OnRevertClick();
        if (GUILayout.Button("Repo browser"))
            OnBrowserClick();

        EditorGUILayout.Separator();
        GUI.enabled |= Selection.activeObject != null && SomeSelectedFileExist();

        if (GUILayout.Button("Commit selected"))
            OnCommitSelectedFileClick();

        if (GUILayout.Button("Revert selected"))
            OnRevertSelectedFileClick();
    }
}
