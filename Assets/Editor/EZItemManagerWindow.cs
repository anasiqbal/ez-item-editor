using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EZItemManagerWindow : EZManagerWindowBase
{
    private const string menuItemLocation = rootMenuLocation + "/EZ Item Manager";

    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<EZItemManagerWindow>(false, "EZ Item Manager");
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
        GUILayout.Label("Template:", GUILayout.Width(80));
        int index = EditorGUILayout.Popup(0, EZItemManager.ItemTemplates.Keys.ToArray(), GUILayout.Width(100));
        if (GUILayout.Button("Create Item"))
            Create(index);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("All Items");
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        foreach (KeyValuePair<string, object> item in EZItemManager.AllItems)
        {
            if (DrawFoldout(string.Format("Item #{0}", item.Key), item.Key))
                DrawEntry(item.Value);
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
        EZItemManager.Save();
    }

    protected override void Create(object data)
    {

    }
}
