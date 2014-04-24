using UnityEngine;
using UnityEditor;
using System;
using EZExtensionMethods;

public class EZPreferences : EditorWindow {

    private const string menuItemLocation = EZManagerWindowBase.rootMenuLocation + "/Preferences";
    private GUIStyle headerStyle = null;

    private Color32 createDataColor;
    private Color32 defineDataColor;
    private Color32 highlightColor;

    private string dataFilePath;
    private string defineDataFilePath;

    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        var window = EditorWindow.GetWindow<EZPreferences>(true, "Game Data Editor Preferences");
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
        dataFilePath = EditorPrefs.GetString(EZConstants.CreateDataFileKey, EZConstants.CreateDataFile);
        defineDataFilePath = EditorPrefs.GetString(EZConstants.DefineDataFileKey, EZConstants.DefineDataFile);
        
        string color = EditorPrefs.GetString(EZConstants.CreateDataColorKey, EZConstants.CreateDataColor);
        createDataColor = color.ToColor();
        
        color = EditorPrefs.GetString(EZConstants.DefineDataColorKey, EZConstants.DefineDataColor);
        defineDataColor = color.ToColor();
        
        color = EditorPrefs.GetString(EZConstants.HighlightColorKey, EZConstants.HighlightColor);
        highlightColor = color.ToColor();
    }

    void LoadDefaults()
    {
        dataFilePath = EZConstants.CreateDataFile;
        defineDataFilePath = EZConstants.DefineDataFile;

        createDataColor = EZConstants.CreateDataColor.ToColor();
        defineDataColor = EZConstants.DefineDataColor.ToColor();
        highlightColor = EZConstants.HighlightColor.ToColor();

        SavePreferences();
    }

    void SavePreferences()
    {
        EditorPrefs.SetString(EZConstants.CreateDataFileKey, dataFilePath);
        EditorPrefs.SetString(EZConstants.DefineDataFileKey, defineDataFilePath);

        EditorPrefs.SetString(EZConstants.CreateDataColorKey, "#" + createDataColor.ToHexString());
        EditorPrefs.SetString(EZConstants.DefineDataColorKey, "#" + defineDataColor.ToHexString());
        EditorPrefs.SetString(EZConstants.HighlightColorKey, "#" + highlightColor.ToHexString());
    }
}

