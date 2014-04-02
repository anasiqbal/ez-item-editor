using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EZItemManagerWindow : EditorWindow
{
    private const string menuItemLocation = "Assets/EZ Item Manager";
    private static Dictionary<string, object> itemDictionary;

    private static Dictionary<string, bool> foldoutState = new Dictionary<string, bool>();

    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<EZItemManagerWindow>(false, "EZ Item Manager");

        itemDictionary = EZItemManager.AllItems;
    }

    [MenuItem(menuItemLocation, true)]
    private static bool showEditorValidator()
    {
        return true;
    }

    void OnGUI()
    {
        DrawHeader();

        if (itemDictionary == null)
            itemDictionary = EZItemManager.AllItems;

        foreach (KeyValuePair<string, object> item in itemDictionary)
        {
            EditorGUILayout.BeginHorizontal();

            bool currentFoldoutState;
            if (!foldoutState.TryGetValue(item.Key, out currentFoldoutState))
                currentFoldoutState = false;

            bool newFoldoutState = EditorGUILayout.Foldout(currentFoldoutState, string.Format("Item #{0}", item.Key));
            if (foldoutState.ContainsKey(item.Key))
                foldoutState [item.Key] = newFoldoutState;
            else
                foldoutState.Add(item.Key, newFoldoutState);

            EditorGUILayout.EndHorizontal();

            if (currentFoldoutState)
                DrawItem(item.Value);
        }
    }

    private static void DrawItem(object data)
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(data as string);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private static void DrawHeader()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Load Items"))
            LoadItems();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save Items"))
            SaveItems();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Create Template"))
            CreateTemplate();

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
        GUILayout.Label("Template:", GUILayout.Width(80));
        int index = EditorGUILayout.Popup(0, EZItemManager.ItemTemplates.Keys.ToArray(), GUILayout.Width(100));
        if (GUILayout.Button("Create Item"))
            CreateItem(index);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("All Items");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private static void LoadItems()
    {
        EZItemManager.Load();
        itemDictionary = EZItemManager.AllItems;
    }

    private static void SaveItems()
    {
        EZItemManager.Save();
        itemDictionary = EZItemManager.AllItems;
    }

    private static void CreateTemplate()
    {
        EditorWindow.GetWindow<EZTemplateManagerWindow>(false, "EZ Item Template Manager");
    }

    private static void CreateItem(int templateIndex)
    {

    }
}
