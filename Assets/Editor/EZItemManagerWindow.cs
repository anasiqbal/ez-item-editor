﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EZItemManagerWindow : EZManagerWindowBase
{
    private const string menuItemLocation = rootMenuLocation + "/EZ Item Manager";

    private string[] templateKeys = null;
    private string newItemName = "";
    private int templateIndex = 0;

    private List<string> deletedItems = new List<string>();

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

    #region OnGUI Method
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

        DrawExpandCollapseAllFoldout(EZItemManager.AllItems.Keys.ToArray());

        EditorGUILayout.EndVertical();
        
        foreach (KeyValuePair<string, object> item in EZItemManager.AllItems)
        {
            DrawEntry(item.Key, item.Value);
        }
        
        //Remove any items that were deleted
        foreach(string deletedkey in deletedItems)
        {
            EZItemManager.RemoveItem(deletedkey);
        }
        deletedItems.Clear();
    }
    #endregion

    #region DrawEntry Method
    protected override void DrawEntry(string key, object data)
    {
        Dictionary<string, object> entry = data as Dictionary<string, object>;
        string templateType = "<unknown>";
        object temp;
        
        if (entry.TryGetValue(EZConstants.TemplateKey, out temp))
            templateType = temp as string;
        
        if (DrawFoldout(string.Format("{0}: {1}", templateType, key), key))
        {
            EditorGUILayout.BeginVertical();

            foreach(string entry_key in entry.Keys.ToArray())
            {
                if (entry_key.Contains(EZConstants.ValuePrefix) ||
                    entry_key.Contains(EZConstants.TemplateKey))
                    continue;
                
                string fieldType = entry[entry_key].ToString();
                if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
                    fieldType = fieldType.ToLower();
                
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Space(20);
                
                EditorGUILayout.LabelField(fieldType, GUILayout.Width(50));
                EditorGUILayout.LabelField(entry_key, GUILayout.Width(100));
                EditorGUILayout.LabelField("Value:", GUILayout.Width(40));
                
                switch(fieldType)
                {
                    case "int":
                        DrawInt(entry_key, entry);
                        break;
                    case "float":
                        DrawFloat(entry_key, entry);
                        break;
                    case "string":
                        DrawString(entry_key, entry);
                        break;

                    default:
                    {
                        List<string> itemKeys = new List<string>();
                        itemKeys.Add("null");

                        // Build a list of possible custom field values
                        // All items that match the template type of the custom field type
                        // will be added to the selection list
                        foreach(KeyValuePair<string, object> item in EZItemManager.AllItems)
                        {
                            string itemType = "<unknown>";
                            Dictionary<string, object> itemData = item.Value as Dictionary<string, object>;

                            if (itemData.TryGetValue(EZConstants.TemplateKey, out temp))
                                itemType = temp as string;

                            if (item.Key.Equals(key) || !itemType.Equals(fieldType))
                                continue;

                            itemKeys.Add(item.Key);
                        }
                        DrawCustom(entry_key, entry, true, itemKeys);
                        break;
                    }
                }
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete"))
                deletedItems.Add(key);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
                      
            GUILayout.Box("", new GUILayoutOption[]
                          {
                GUILayout.ExpandWidth(true),
                GUILayout.Height(1)
            });
            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();
        }
    }
    #endregion

    #region Load/Save/Create Item Methods
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
            Dictionary<string, object> itemData = new Dictionary<string, object>(templateData);
            itemData.Add(EZConstants.TemplateKey, templateKey);

            EZItemManager.AddItem(itemName, itemData);
            foldoutState[itemName] = true;
        }
        else
            Debug.LogError("Template data not found: " + templateKey);
    }
    #endregion
}
