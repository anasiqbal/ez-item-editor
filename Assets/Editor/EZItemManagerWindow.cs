using UnityEngine;
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

        GUILayout.Space(EZConstants.IndentSize);

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
                if (entry_key.StartsWith(EZConstants.ValuePrefix) ||
                    entry_key.StartsWith(EZConstants.IsListPrefix) ||
                    entry_key.StartsWith(EZConstants.TemplateKey))
                    continue;
                
                if (entry.ContainsKey(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, entry_key)))
                    DrawListField(key, entry_key, entry);
                else
                    DrawSingleField(key, entry_key, entry);
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

    void DrawSingleField(string key, string entry_key, Dictionary<string, object> entry)
    {
        string fieldType = entry[entry_key].ToString();
        if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
            fieldType = fieldType.ToLower();
        
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Space(EZConstants.IndentSize);
        
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
                List<string> itemKeys = GetPossibleCustomValues(key, fieldType);
                DrawCustom(entry_key, entry, true, itemKeys);
                break;
            }
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    void DrawListField(string template_key, string entry_key, Dictionary<string, object> entry)
    {
        try
        {
            string foldoutKey = string.Format(EZConstants.MetaDataFormat, template_key, entry_key);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);
            
            string fieldType = entry[entry_key].ToString();
            if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
                fieldType = fieldType.ToLower();
            
            EditorGUILayout.BeginVertical();       
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EZConstants.IndentSize);
            
            newFoldoutState = EditorGUILayout.Foldout(currentFoldoutState, string.Format("List<{0}>   {1}", fieldType, entry_key));
            if (newFoldoutState != currentFoldoutState)
            {
                if (newFoldoutState)
                    listFieldFoldoutState.Add(foldoutKey);
                else
                    listFieldFoldoutState.Remove(foldoutKey);
            }
            
            object temp = null;
            List<object> list = null;
            
            if (entry.TryGetValue(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, entry_key), out temp))
                list = temp as List<object>;
            
            GUILayout.Space(120);
            EditorGUILayout.LabelField("Count:", GUILayout.Width(40));
            
            newListCount = EditorGUILayout.IntField(newListCount, GUILayout.Width(40));
            if (newListCount != list.Count && GUILayout.Button("Resize"))            
                ResizeList(list, newListCount, 0);
            
            GUILayout.Space(20);
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            
            if (newFoldoutState)
            {
                for (int i = 0; i < list.Count; i++) 
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(EZConstants.IndentSize*2);
                    
                    EditorGUILayout.LabelField(string.Format("{0}:", i), GUILayout.Width(20));
                    
                    switch (fieldType) {
                        case "int":
                            DrawListInt(i, Convert.ToInt32(list[i]), list);
                            break;
                        case "float":
                            DrawListFloat(i, Convert.ToSingle(list[i]), list);
                            break;
                        case "string":
                            DrawListString(i, list[i] as string, list);
                            break;
                            
                        default:
                            List<string> itemKeys = GetPossibleCustomValues(template_key, fieldType);
                            DrawListCustom(i, list[i] as string, list, true, itemKeys);
                            break;
                    }
                    
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    List<string> GetPossibleCustomValues(string key, string fieldType)
    {
        object temp;
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

        return itemKeys;
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
            SetFoldout(true, itemName);
        }
        else
            Debug.LogError("Template data not found: " + templateKey);
    }
    #endregion
}
