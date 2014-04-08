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

    private string[] filterTemplateKeys = null;
    private int filterTemplateIndex = 0;

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
        {
            templateKeys = EZItemManager.ItemTemplates.Keys.ToArray();

            List<string> temp = EZItemManager.ItemTemplates.Keys.ToList();
            temp.Add("_All");
            temp.Sort();
            filterTemplateKeys = temp.ToArray();
        }

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

        verticalScrollbarPosition = EditorGUILayout.BeginScrollView(verticalScrollbarPosition);
        foreach (KeyValuePair<string, Dictionary<string, object>> item in EZItemManager.AllItems)
        {
            DrawEntry(item.Key, item.Value);
        }
        EditorGUILayout.EndScrollView();
        
        //Remove any items that were deleted
        foreach(string deletedkey in deletedItems)
        {
            EZItemManager.RemoveItem(deletedkey);
        }
        deletedItems.Clear();
    }
    #endregion

    #region Draw Filter/Search Override
    protected override void DrawFilterSection()
    {
        base.DrawFilterSection();
        
        // Filter dropdown
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Filter By Template Type:", GUILayout.Width(140));
        filterTemplateIndex = EditorGUILayout.Popup(filterTemplateIndex, filterTemplateKeys, GUILayout.Width(100));
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region DrawEntry Method
    protected override void DrawEntry(string key, Dictionary<string, object> data)
    {
        string templateType = "<unknown>";
        object temp;
        
        if (data.TryGetValue(EZConstants.TemplateKey, out temp))
            templateType = temp as string;

        // Return if we don't match any of the filter types
        if (filterTemplateIndex != -1 &&
            filterTemplateIndex < filterTemplateKeys.Length &&
            !filterTemplateKeys[filterTemplateIndex].Equals("_All") &&
            !templateType.Equals(filterTemplateKeys[filterTemplateIndex]))
            return;

        bool templateKeyMatch = templateType.ToLower().Contains(filterText.ToLower());
        bool fieldKeyMatch = !EZItemManager.ShouldFilterByField(templateType, filterText);
        
        // Return if the template keys don't contain the filter text or
        // if the template fields don't contain the filter text
        if (!templateKeyMatch && !fieldKeyMatch)
            return;

        // Start drawing below
        if (DrawFoldout(string.Format("{0}: {1}", templateType, key), key))
        {
            bool shouldDrawSpace = false;
            bool didDrawSpaceForSection = false;

            EditorGUILayout.BeginVertical();
            
            // Draw the basic types
            foreach(BasicFieldType fieldType in Enum.GetValues(typeof(BasicFieldType)))
            {
                List<string> fieldKeys = EZItemManager.ItemFieldKeysOfType(key, fieldType.ToString());                
                foreach(string fieldKey in fieldKeys)
                {
                    DrawSingleField(templateType, fieldKey, data);
                    shouldDrawSpace = true;
                }
            }
            
            // Draw the custom types
            foreach(string fieldKey in EZItemManager.ItemCustomFieldKeys(key))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    GUILayout.Space(10);
                    didDrawSpaceForSection = true;
                }
                
                shouldDrawSpace = true;
                DrawSingleField(templateType, fieldKey, data);
            }
            didDrawSpaceForSection = false;
            
            // Draw the lists
            foreach(BasicFieldType fieldType in Enum.GetValues(typeof(BasicFieldType)))
            {
                List<string> fieldKeys = EZItemManager.ItemListFieldKeysOfType(key, fieldType.ToString());                
                foreach(string fieldKey in fieldKeys)
                {
                    if (shouldDrawSpace && !didDrawSpaceForSection)
                    {
                        GUILayout.Space(10);
                        didDrawSpaceForSection = true;
                    }
                    
                    shouldDrawSpace = true;
                    DrawListField(templateType, fieldKey, data);
                }
            }
            didDrawSpaceForSection = false;
            
            // Draw the custom lists
            foreach(string fieldKey in EZItemManager.ItemCustomListFields(key))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    GUILayout.Space(10);
                    didDrawSpaceForSection = true;
                }
                
                shouldDrawSpace = true;
                DrawListField(templateType, fieldKey, data);
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

    void DrawSingleField(string templateKey, string fieldKey, Dictionary<string, object> itemData)
    {
        string fieldType = itemData[fieldKey].ToString();
        if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
            fieldType = fieldType.ToLower();
        
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Space(EZConstants.IndentSize);
        
        EditorGUILayout.LabelField(fieldType, GUILayout.Width(50));
        EditorGUILayout.LabelField(fieldKey, GUILayout.Width(100));
        EditorGUILayout.LabelField("Value:", GUILayout.Width(40));
        
        switch(fieldType)
        {
            case "int":
                DrawInt(fieldKey, itemData);
                break;
            case "float":
                DrawFloat(fieldKey, itemData);
                break;
            case "string":
                DrawString(fieldKey, itemData);
                break;
                
            default:
            {
                List<string> itemKeys = GetPossibleCustomValues(templateKey, fieldType);
                DrawCustom(fieldKey, itemData, true, itemKeys);
                break;
            }
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    void DrawListField(string templateKey, string fieldKey, Dictionary<string, object> itemData)
    {
        try
        {
            string foldoutKey = string.Format(EZConstants.MetaDataFormat, templateKey, fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);
            
            string fieldType = itemData[fieldKey].ToString();
            if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
                fieldType = fieldType.ToLower();
            
            EditorGUILayout.BeginVertical();       
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EZConstants.IndentSize);
            
            newFoldoutState = EditorGUILayout.Foldout(currentFoldoutState, string.Format("List<{0}>   {1}", fieldType, fieldKey));
            if (newFoldoutState != currentFoldoutState)
            {
                if (newFoldoutState)
                    listFieldFoldoutState.Add(foldoutKey);
                else
                    listFieldFoldoutState.Remove(foldoutKey);
            }
            
            object temp = null;
            List<object> list = null;
            
            if (itemData.TryGetValue(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldKey), out temp))
                list = temp as List<object>;
            
            GUILayout.Space(120);
            EditorGUILayout.LabelField("Count:", GUILayout.Width(40));

            int newListCount;
            string listCountKey = string.Format(EZConstants.MetaDataFormat, templateKey, fieldKey);
            if (newListCountDict.ContainsKey(listCountKey))
            {
                newListCount = newListCountDict[listCountKey];
            }
            else
            {
                newListCount = list.Count;
                newListCountDict.Add(listCountKey, newListCount);
            }
            newListCount = EditorGUILayout.IntField(newListCount, GUILayout.Width(40));
            newListCountDict[listCountKey] = newListCount;
            if (newListCount != list.Count && GUILayout.Button("Resize"))            
            {
                ResizeList(list, newListCount, 0);
                newListCountDict[listCountKey] = newListCount;
            }
            
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
                            List<string> itemKeys = GetPossibleCustomValues(templateKey, fieldType);
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

    List<string> GetPossibleCustomValues(string fieldKey, string fieldType)
    {
        object temp;
        List<string> itemKeys = new List<string>();
        itemKeys.Add("null");
        
        // Build a list of possible custom field values
        // All items that match the template type of the custom field type
        // will be added to the selection list
        foreach(KeyValuePair<string, Dictionary<string, object>> item in EZItemManager.AllItems)
        {
            string itemType = "<unknown>";
            Dictionary<string, object> itemData = item.Value as Dictionary<string, object>;
            
            if (itemData.TryGetValue(EZConstants.TemplateKey, out temp))
                itemType = temp as string;
            
            if (item.Key.Equals(fieldKey) || !itemType.Equals(fieldType))
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

        Dictionary<string, object> templateData = null;       
        if (EZItemManager.ItemTemplates.TryGetValue(templateKey, out templateData))
        {
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
