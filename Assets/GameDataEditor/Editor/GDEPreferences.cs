using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class GDEPreferences : EditorWindow {

    private const string menuItemLocation = GDEManagerWindowBase.rootMenuLocation + "/Preferences";
    private GUIStyle headerStyle = null;

    private Color32 createDataColor;
    private Color32 defineDataColor;
    private Color32 highlightColor;

    private string dataFilePath;

    [MenuItem(menuItemLocation, false, GDEManagerWindowBase.menuItemStartPriority)]
    private static void showEditor()
    {
        var window = EditorWindow.GetWindow<GDEPreferences>(true, "Game Data Editor Preferences");
        window.LoadPreferences();
        window.Show();
    }
        
    void OnGUI()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.fontSize = 16;
        }

        GUIContent content = new GUIContent("File Locations");
        Vector2 size = headerStyle.CalcSize(content);
        EditorGUILayout.LabelField("File Locations", headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Create Data File", GUILayout.Width(100));
        dataFilePath = EditorGUILayout.TextField(dataFilePath);
        if (GUILayout.Button("Browse ...", GUILayout.Width(70)))
        {
            string newDataFilePath = EditorUtility.OpenFilePanel("Open Data File", dataFilePath, "json");
            if (!string.IsNullOrEmpty(newDataFilePath) && !newDataFilePath.Equals(dataFilePath))
                dataFilePath = newDataFilePath;
            GUI.FocusControl("");
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        content.text = "Colors";
        size = headerStyle.CalcSize(content);
        EditorGUILayout.LabelField("Colors", headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));

        createDataColor = EditorGUILayout.ColorField("Create Data Header", createDataColor);
        defineDataColor = EditorGUILayout.ColorField("Define Data Header", defineDataColor);
        highlightColor = EditorGUILayout.ColorField("Highlight", highlightColor);

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Use Defaults"))
            LoadDefaults();

        if (GUILayout.Button("Apply"))
            SavePreferences();

        EditorGUILayout.EndHorizontal();
    }

    void LoadPreferences()
    {
        dataFilePath = EditorPrefs.GetString(GDEConstants.DataFileKey, Application.dataPath + "/" + GDEConstants.DataFile);

        string color = EditorPrefs.GetString(GDEConstants.CreateDataColorKey, GDEConstants.CreateDataColor);
        createDataColor = color.ToColor();
        
        color = EditorPrefs.GetString(GDEConstants.DefineDataColorKey, GDEConstants.DefineDataColor);
        defineDataColor = color.ToColor();
        
        color = EditorPrefs.GetString(GDEConstants.HighlightColorKey, GDEConstants.HighlightColor);
        highlightColor = color.ToColor();
    }

    void LoadDefaults()
    {
        dataFilePath = Application.dataPath + "/" + GDEConstants.DataFile;

        createDataColor = GDEConstants.CreateDataColor.ToColor();
        defineDataColor = GDEConstants.DefineDataColor.ToColor();
        highlightColor = GDEConstants.HighlightColor.ToColor();

        GUI.FocusControl("");

        SavePreferences();
    }

    void SavePreferences()
    {
        EditorPrefs.SetString(GDEConstants.CreateDataColorKey, "#" + createDataColor.ToHexString());
        EditorPrefs.SetString(GDEConstants.DefineDataColorKey, "#" + defineDataColor.ToHexString());
        EditorPrefs.SetString(GDEConstants.HighlightColorKey, "#" + highlightColor.ToHexString());

        string dataFileDirectory = dataFilePath.Replace(Path.GetFileName(dataFilePath), "");
        if (Directory.Exists(dataFileDirectory))
        {
            EditorPrefs.SetString(GDEConstants.DataFileKey, dataFilePath);
            GDEItemManager.Load(true);
            GUI.FocusControl("");
        }
        else
        {
            EditorUtility.DisplayDialog(GDEStrings.ErrorLbl, string.Format(GDEStrings.DirectoryNotFound, dataFileDirectory), GDEStrings.OkLbl);
        }
    }
}

