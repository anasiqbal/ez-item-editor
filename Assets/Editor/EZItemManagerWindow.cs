using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EZItemManagerWindow : EZManagerWindowBase
{
    private const string menuItemLocation = rootMenuLocation + "/EZ Item Manager";

    private string[] schemaKeys = null;
    private string newItemName = "";
    private int schemaIndex = 0;

    private string[] filterSchemaKeys = null;
    private int filterSchemaIndex = 0;

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
        if (schemaKeys == null || schemaKeys.Length != EZItemManager.AllSchemas.Keys.Count)
        {
            schemaKeys = EZItemManager.AllSchemas.Keys.ToArray();

            List<string> temp = EZItemManager.AllSchemas.Keys.ToList();
            temp.Add("_All");
            temp.Sort();
            filterSchemaKeys = temp.ToArray();
        }

        base.OnGUI();

        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Schema:", GUILayout.Width(60));
        schemaIndex = EditorGUILayout.Popup(schemaIndex, schemaKeys, GUILayout.Width(100));

        GUILayout.Space(EZConstants.IndentSize);

        EditorGUILayout.LabelField("Item Name:", GUILayout.Width(65));
        newItemName = EditorGUILayout.TextField(newItemName);

        if (GUILayout.Button("Create New Item"))
        {
            List<object> args = new List<object>();
            args.Add(schemaKeys[schemaIndex]);
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
        EditorGUILayout.LabelField("Filter By Schema:", GUILayout.Width(140));
        filterSchemaIndex = EditorGUILayout.Popup(filterSchemaIndex, filterSchemaKeys, GUILayout.Width(100));
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region DrawEntry Method
    protected override void DrawEntry(string key, Dictionary<string, object> data)
    {
        string schemaType = "<unknown>";
        object temp;
        
        if (data.TryGetValue(EZConstants.SchemaKey, out temp))
            schemaType = temp as string;

        // Return if we don't match any of the filter types
        if (filterSchemaIndex != -1 &&
            filterSchemaIndex < filterSchemaKeys.Length &&
            !filterSchemaKeys[filterSchemaIndex].Equals("_All") &&
            !schemaType.Equals(filterSchemaKeys[filterSchemaIndex]))
            return;

        bool schemaKeyMatch = schemaType.ToLower().Contains(filterText.ToLower());
        bool fieldKeyMatch = !EZItemManager.ShouldFilterByField(schemaType, filterText);
        
        // Return if the schema keys don't contain the filter text or
        // if the schema fields don't contain the filter text
        if (!schemaKeyMatch && !fieldKeyMatch)
            return;

        // Start drawing below
        if (DrawFoldout(string.Format("{0}: {1}", schemaType, key), key))
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
                    DrawSingleField(schemaType, fieldKey, data);
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
                DrawSingleField(schemaType, fieldKey, data);
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
                    DrawListField(schemaType, fieldKey, data);
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
                DrawListField(schemaType, fieldKey, data);
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

    void DrawSingleField(string schemaKey, string fieldKey, Dictionary<string, object> itemData)
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
                List<string> itemKeys = GetPossibleCustomValues(schemaKey, fieldType);
                DrawCustom(fieldKey, itemData, true, itemKeys);
                break;
            }
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    void DrawListField(string schemaKey, string fieldKey, Dictionary<string, object> itemData)
    {
        try
        {
            string foldoutKey = string.Format(EZConstants.MetaDataFormat, schemaKey, fieldKey);
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
            string listCountKey = string.Format(EZConstants.MetaDataFormat, schemaKey, fieldKey);
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
                            List<string> itemKeys = GetPossibleCustomValues(schemaKey, fieldType);
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
        // All items that match the schema type of the custom field type
        // will be added to the selection list
        foreach(KeyValuePair<string, Dictionary<string, object>> item in EZItemManager.AllItems)
        {
            string itemType = "<unknown>";
            Dictionary<string, object> itemData = item.Value as Dictionary<string, object>;
            
            if (itemData.TryGetValue(EZConstants.SchemaKey, out temp))
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
        EZItemManager.LoadSchemas();
    }

    protected override void Save()
    {
        EZItemManager.SaveItems();
    }

    protected override void Create(object data)
    {
        List<object> args = data as List<object>;
        string schemaKey = args[0] as string;
        string itemName = args[1] as string;

        Dictionary<string, object> schemaData = null;       
        if (EZItemManager.AllSchemas.TryGetValue(schemaKey, out schemaData))
        {
            Dictionary<string, object> itemData = new Dictionary<string, object>(schemaData);
            itemData.Add(EZConstants.SchemaKey, schemaKey);

            EZItemManager.AddItem(itemName, itemData);
            SetFoldout(true, itemName);
        }
        else
            Debug.LogError("Schema data not found: " + schemaKey);
    }
    #endregion
}
