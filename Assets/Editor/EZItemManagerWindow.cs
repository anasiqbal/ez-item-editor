using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EZItemManagerWindow : EditorWindow
{
    private const string menuItemLocation = "Assets/EZ Item Manager";
    private static Dictionary<string, object> itemDictionary;

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
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("All Items");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        TestMe();

        foreach (object item in itemDictionary.Values)
        {
            DrawItem(item);
        }
    }

    private static void TestMe()
    {
        if (itemDictionary == null)
        {
            itemDictionary = new Dictionary<string, object>();
            itemDictionary.Add("1", "Item details go here....");
            itemDictionary.Add("2", "Item details go here....");
            itemDictionary.Add("3", "Item details go here....");
        }
    }

    private static void DrawItem(object data)
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("An Item");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(data as string);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}
