using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EZItemManagerWindow : EZManagerWindowBase
{
    private const string menuItemLocation = rootMenuLocation + "/EZ Item Manager";

    private string[] templateKeys = null;
    private string newItemName = "";
    private int templateIndex = 0;

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
        if (templateKeys == null || templateKeys.Length != EZItemManager.ItemTemplates.Keys.Count)
            templateKeys = EZItemManager.ItemTemplates.Keys.ToArray();

        base.OnGUI();

        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Template:", GUILayout.Width(60));
        templateIndex = EditorGUILayout.Popup(templateIndex, templateKeys, GUILayout.Width(100));

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Item Name:", GUILayout.Width(65));
        newItemName = EditorGUILayout.TextField(newItemName);

        if (GUILayout.Button("Create New Item"))
        {
            List<object> args = new List<object>();
            args.Add(templateKeys[templateIndex]);
            args.Add(newItemName);

            Create(args);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        EditorGUILayout.EndVertical();
        
        foreach (KeyValuePair<string, object> item in EZItemManager.AllItems)
        {
            if (DrawFoldout(string.Format("Item: {0}", item.Key), item.Key))
                DrawEntry(item.Key, item.Value);
        }
    }

    protected override void DrawEntry(string key, object data)
    {
        EditorGUILayout.BeginVertical();
        
        Dictionary<string, object> entry = data as Dictionary<string, object>;
        
        foreach(string entry_key in entry.Keys.ToArray())
        {
            if (entry_key.StartsWith(EZConstants.ValuePrefix))
                continue;
            
            string fieldType = entry[entry_key].ToString();
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Space(20);
            
            EditorGUILayout.LabelField(fieldType.ToLower(), GUILayout.Width(50));
            EditorGUILayout.LabelField(entry_key, GUILayout.Width(100));
            EditorGUILayout.LabelField("Value:", GUILayout.Width(40));
            
            switch(fieldType)
            {
                case "Int":
                    DrawInt(entry_key, entry);
                    break;
                case "Float":
                    DrawFloat(entry_key, entry);
                    break;
                case "String":
                    DrawString(entry_key, entry);
                    break;
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(20);
                  
        GUILayout.Box("", new GUILayoutOption[]
                      {
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1)
        });
        EditorGUILayout.Separator();
        EditorGUILayout.EndVertical();
    }

    protected override void Load()
    {
        EZItemManager.LoadItems();
        EZItemManager.LoadTemplates();
    }

    protected override void Save()
    {
        EZItemManager.SaveItems();
    }

    protected override void Create(object data)
    {
        List<object> args = data as List<object>;
        string templateKey = args[0] as string;
        string itemName = args[1] as string;

        object temp = null;
        Dictionary<string, object> templateData = null;
       
        if (EZItemManager.ItemTemplates.TryGetValue(templateKey, out temp))
        {
            templateData = temp as Dictionary<string, object>;
            EZItemManager.AddItem(itemName, new Dictionary<string, object>(templateData));
            foldoutState[itemName] = true;
        }
        else
            Debug.LogError("Template data not found: " + templateKey);
    }
}
