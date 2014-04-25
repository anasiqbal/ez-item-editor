using UnityEngine;
using UnityEditor;
using System;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class GDEPreferences : EditorWindow {

    private const string menuItemLocation = GDEManagerWindowBase.rootMenuLocation + "/Preferences";
    private GUIStyle headerStyle = null;

    private Color32 createDataColor;
    private Color32 defineDataColor;
    private Color32 highlightColor;

    private string dataFilePath;
    private string defineDataFilePath;

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

        dataFilePath = EditorGUILayout.TextField("Create Data File", dataFilePath);
        defineDataFilePath = EditorGUILayout.TextField("Define Data File", defineDataFilePath);

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
        dataFilePath = EditorPrefs.GetString(GDEConstants.CreateDataFileKey, GDEConstants.CreateDataFile);
        defineDataFilePath = EditorPrefs.GetString(GDEConstants.DefineDataFileKey, GDEConstants.DefineDataFile);
        
        string color = EditorPrefs.GetString(GDEConstants.CreateDataColorKey, GDEConstants.CreateDataColor);
        createDataColor = color.ToColor();
        
        color = EditorPrefs.GetString(GDEConstants.DefineDataColorKey, GDEConstants.DefineDataColor);
        defineDataColor = color.ToColor();
        
        color = EditorPrefs.GetString(GDEConstants.HighlightColorKey, GDEConstants.HighlightColor);
        highlightColor = color.ToColor();
    }

    void LoadDefaults()
    {
        dataFilePath = GDEConstants.CreateDataFile;
        defineDataFilePath = GDEConstants.DefineDataFile;

        createDataColor = GDEConstants.CreateDataColor.ToColor();
        defineDataColor = GDEConstants.DefineDataColor.ToColor();
        highlightColor = GDEConstants.HighlightColor.ToColor();

        SavePreferences();
    }

    void SavePreferences()
    {
        EditorPrefs.SetString(GDEConstants.CreateDataFileKey, dataFilePath);
        EditorPrefs.SetString(GDEConstants.DefineDataFileKey, defineDataFilePath);

        EditorPrefs.SetString(GDEConstants.CreateDataColorKey, "#" + createDataColor.ToHexString());
        EditorPrefs.SetString(GDEConstants.DefineDataColorKey, "#" + defineDataColor.ToHexString());
        EditorPrefs.SetString(GDEConstants.HighlightColorKey, "#" + highlightColor.ToHexString());
    }
}

