using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MultiplatformBuilding : EditorWindow
{
    [Serializable]
    class SerializableEditorBuildSettingsScene
    {
        public string path;
        public bool enabled;

        public static explicit operator EditorBuildSettingsScene(SerializableEditorBuildSettingsScene serialized)
        {
            return new EditorBuildSettingsScene(serialized.path, serialized.enabled);
        }

        public static explicit operator SerializableEditorBuildSettingsScene(EditorBuildSettingsScene nonserialized)
        {
            return new SerializableEditorBuildSettingsScene
            {
                path = nonserialized.path,
                enabled = nonserialized.enabled
            };
        }
    }
    [Serializable]
    class BuildInfo
    {
        public BuildTarget buildTarget;
        public string customProjectName;
        public SerializableEditorBuildSettingsScene[] scenes;
    }
    [Serializable]
    class BuildInfoArray
    {
        public string outputDirectory;
        public BuildInfo[] buildInfo;
    }
    string outputDirectory;

    BuildInfo[] buildInfo;
    EditorBuildSettingsScene[] scenes;

    [MenuItem("Building/Multiplatform")]
    static void Init()
    {
        GetWindow<MultiplatformBuilding>(true);
    }

    private void OnEnable()
    {
        if (EditorPrefs.HasKey("BuildInfo"))
        {
            BuildInfoArray biArr = new BuildInfoArray();
            var json = EditorPrefs.GetString("BuildInfo");
            EditorJsonUtility.FromJsonOverwrite(json, biArr);
            buildInfo = biArr.buildInfo;
            outputDirectory = biArr.outputDirectory;
        }
        GetScenes();
    }

    private void OnGUI()
    {
        if (buildInfo == null)
        {
            buildInfo = new BuildInfo[0];
        }

        EditorGUILayout.BeginHorizontal();
        outputDirectory = EditorGUILayout.TextField("Output directory", outputDirectory);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            bool isEmpty = string.IsNullOrEmpty(outputDirectory) || string.IsNullOrWhiteSpace(outputDirectory);
            var di = isEmpty ? 
                new DirectoryInfo(Directory.GetCurrentDirectory()) :
                new DirectoryInfo(outputDirectory).Parent;
            var output = EditorUtility.OpenFolderPanel("Output directory", di.FullName, isEmpty ? di.Name + "_Build" : outputDirectory);
            if (!string.IsNullOrEmpty(output))
            {
                outputDirectory = output;
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Add build info"))
        {
            Array.Resize(ref buildInfo, buildInfo.Length + 1);
        }

        for (int i = 0; i < buildInfo.Length; i++)
        {
            EditorGUILayout.BeginVertical("box");
            var bi = buildInfo[i];
            if (bi == null)
            {
                bi = new BuildInfo();
                bi.buildTarget = EditorUserBuildSettings.activeBuildTarget;
                bi.customProjectName = PlayerSettings.productName;
                bi.scenes = scenes.Select(s => (SerializableEditorBuildSettingsScene)s).ToArray();
                buildInfo[i] = bi;
            }
            bi.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup(bi.buildTarget);
            bi.customProjectName = EditorGUILayout.TextField("Custom project name", bi.customProjectName);

            EditorGUILayout.BeginVertical("box");
            for (int i1 = 0; i1 < bi.scenes.Length; i1++)
            {
                EditorGUILayout.BeginHorizontal();
                bi.scenes[i1].enabled = EditorGUILayout.ToggleLeft(bi.scenes[i1].path, bi.scenes[i1].enabled);
                EditorGUILayout.LabelField(i1.ToString(), GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            //bi.scenes = scenes.Where(s => s.enabled).Select(s=>(SerializableEditorBuildSettingsScene)s).ToArray();
            EditorGUILayout.BeginHorizontal();
            var col = GUI.color;
            GUI.color = Color.red;
            if (GUILayout.Button("Remove", GUILayout.Width(100)))
            {
                buildInfo[i] = null;
                List<BuildInfo> l_bi = new List<BuildInfo>(buildInfo);
                l_bi.RemoveAll(b => b == null);
                buildInfo = l_bi.ToArray();
            }
            GUI.color = Color.yellow;
            if (GUILayout.Button("Build", GUILayout.Width(100)))
            {
                if (!string.IsNullOrEmpty(outputDirectory))
                    Build(outputDirectory, buildInfo[i]);
            }
            GUI.color = col;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if(GUI.changed)
        {
            BuildInfoArray biArr = new BuildInfoArray();
            biArr.outputDirectory = outputDirectory;
            biArr.buildInfo = buildInfo;
            var json = EditorJsonUtility.ToJson(biArr);
            EditorPrefs.SetString("BuildInfo", json);
        }
        GUI.enabled = buildInfo.Length > 0 && !string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrWhiteSpace(outputDirectory);
        EditorGUILayout.Space();
        if (GUILayout.Button("Build all"))
        {
            for (int i = 0; i < buildInfo.Length; i++)
            {
                //SetCurrentCompanyScenes();
                //string output = EditorUtility.OpenFolderPanel("Building", new DirectoryInfo(outputDirectory).Parent.FullName, toolbarCompanyNames[currentCompany].text + "_Build");
                if (!string.IsNullOrEmpty(outputDirectory))
                    Build(outputDirectory, buildInfo[i]);
            }
        }
    }

    private void GetScenes()
    {
        string[] dirs = Directory.GetDirectories("Assets/Scenes");

        string[] sceneFiles = Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories);

        sceneFiles = sceneFiles.Where(val => !val.Replace("\\", "/").AfterLast("/").StartsWith("_")).ToArray();

        scenes = new EditorBuildSettingsScene[sceneFiles.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = new EditorBuildSettingsScene(sceneFiles[i], true);
        }

        //EditorBuildSettings.scenes = scenes;
    }

    private static void Build(string output, BuildInfo buildInfo)
    {
        var outputDirectory = output;//.Remove(output.LastIndexOf("/"));
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);
        outputDirectory = new DirectoryInfo(outputDirectory).FullName.Replace("\\", "/");
        var fullpath = Path.Combine(outputDirectory, buildInfo.customProjectName);
        if (!Directory.Exists(fullpath))
            Directory.CreateDirectory(fullpath);
        outputDirectory = new DirectoryInfo(fullpath).FullName.Replace("\\", "/");
        // Get all the scenes
        EditorBuildSettingsScene[] levelList = buildInfo.scenes.Where(s => s.enabled).Select(s=>(EditorBuildSettingsScene)s).ToArray();
        string scenesNames = "(";
        foreach (var s in levelList)
            scenesNames += s.path.Remove(s.path.IndexOf(".unity")) + ", ";
        if (scenesNames.Length <= 1)
        {
            Debug.LogError("No scenes found! Please add scenes (Files -> Build Settings -> Scenes in build");
            return;
        }
        scenesNames = scenesNames.Remove(scenesNames.Length - 2) + ")";
        PlayerSettings.productName = buildInfo.customProjectName;

        Debug.Log("Building Platform: " + buildInfo.buildTarget.ToString());
        Debug.Log("Building Target: " + outputDirectory);
        Debug.Log("Scenes Processed: " + levelList.Length);
        Debug.Log("Scenes Names: " + scenesNames);
        // Build the project
        var ext = ".exe";
        if (buildInfo.buildTarget == BuildTarget.Android)
            ext = ".apk";
        var results = BuildPipeline.BuildPlayer(levelList, $"{outputDirectory}/{buildInfo.customProjectName}{ext}", buildInfo.buildTarget, BuildOptions.None);
        if (results.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build complete successfuly!");
        }
        else
            Debug.LogError("Build Error:" + results.summary.result);
    }
}
