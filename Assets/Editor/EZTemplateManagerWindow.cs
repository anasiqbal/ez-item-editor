using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EZTemplateManagerWindow : EditorWindow {

    private const string menuItemLocation = "Assets/EZ Item Template Manager";
    private static Dictionary<string, object> templateDictionary;

    private static Dictionary<string, bool> foldoutState = new Dictionary<string, bool>();
    private static string newTemplateName = "";
    
    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<EZTemplateManagerWindow>(false, "EZ Item Template Manager");
        templateDictionary = EZItemManager.ItemTemplates;
    }
    
    [MenuItem(menuItemLocation, true)]
    private static bool showEditorValidator()
    {
        return true;
    }
    
    void OnGUI()
    {
        DrawHeader();

        if (templateDictionary == null)
            templateDictionary = EZItemManager.ItemTemplates;

        foreach(KeyValuePair<string, object> template in templateDictionary)
        {
            EditorGUILayout.BeginHorizontal();
            
            bool currentFoldoutState;
            if (!foldoutState.TryGetValue(template.Key, out currentFoldoutState))
                currentFoldoutState = false;
            
            bool newFoldoutState = EditorGUILayout.Foldout(currentFoldoutState, string.Format("Template: {0}", template.Key));
            if (foldoutState.ContainsKey(template.Key))
                foldoutState [template.Key] = newFoldoutState;
            else
                foldoutState.Add(template.Key, newFoldoutState);
            
            EditorGUILayout.EndHorizontal();
            
            if (currentFoldoutState)
                DrawTemplate(template.Value);
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Load Templates"))
            LoadTemplates();
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Save Templates"))
            SaveTemplates();

        GUILayout.FlexibleSpace();
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        GUILayout.Box("", new GUILayoutOption[]
                      {
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1)
        });
        EditorGUILayout.Separator();
        
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Template Name:", GUILayout.Width(100));
        newTemplateName = EditorGUILayout.TextField(newTemplateName);
        if (GUILayout.Button("Create New Template"))
            CreateTemplate(newTemplateName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("All Templates");
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    private void DrawTemplate(object data)
    {
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(data as string);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    private void LoadTemplates()
    {
        EZItemManager.Load();
        templateDictionary = EZItemManager.ItemTemplates;
    }

    private void SaveTemplates()
    {

    }

    private void CreateTemplate(string name)
    {
        EZItemManager.AddTemplate(name, "This was a new template....");
    }
}
