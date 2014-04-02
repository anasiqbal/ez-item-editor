using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EZTemplateManagerWindow : EZManagerWindowBase {

    private const string menuItemLocation = rootMenuLocation + "/EZ Template Manager";
    private static string newTemplateName = "";
    
    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<EZTemplateManagerWindow>(false, "EZ Template Manager");
    }
    
    [MenuItem(menuItemLocation, true)]
    private static bool showEditorValidator()
    {
        return true;
    }
    
    protected override void OnGUI()
    {
        base.OnGUI();

        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Template Name:", GUILayout.Width(100));
        newTemplateName = EditorGUILayout.TextField(newTemplateName);
        if (GUILayout.Button("Create New Template"))
            Create(newTemplateName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("All Templates");
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();

        foreach(KeyValuePair<string, object> template in EZItemManager.ItemTemplates)
        {   
            if (DrawFoldout(string.Format("Template: {0}", template.Key), template.Key))
                DrawEntry(template.Value);
        }
    }

    protected override void DrawEntry(object data)
    {
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(data as string);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    protected override void Load()
    {
        EZItemManager.Load();
    }

    protected override void Save()
    {

    }

    protected override void Create(object data)
    {
        EZItemManager.AddTemplate(data as string, "This was a new template....");
    }
}
