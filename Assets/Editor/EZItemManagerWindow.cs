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

        NewLine();

        float width = 60;
        GUI.Label(new Rect(currentLinePosition, EZConstants.LineHeight*currentLine, width, StandardHeight()), "Schema:");
        currentLinePosition += (width + 2);

        width = 100;
        schemaIndex = EditorGUI.Popup(new Rect(currentLinePosition, EZConstants.LineHeight*currentLine, width, StandardHeight()), schemaIndex, schemaKeys);

        NewLine();

        width = 65;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Item Name:");
        currentLinePosition += (width + 8);

        width = 180;
        newItemName = EditorGUI.TextField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), newItemName);
        currentLinePosition += (width + 4);

        width = 100;
        if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Create New Item"))
        {
            List<object> args = new List<object>();
            args.Add(schemaKeys[schemaIndex]);
            args.Add(newItemName);

            Create(args);
        }

        NewLine();

        DrawExpandCollapseAllFoldout(EZItemManager.AllItems.Keys.ToArray());

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
        float width = 100;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Filter By Schema:");
        currentLinePosition += (width + 2);

        width = 100;
        filterSchemaIndex = EditorGUI.Popup(new Rect(currentLinePosition, PopupTop(), width, StandardHeight()), filterSchemaIndex, filterSchemaKeys);

        NewLine();
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

            // Draw the basic types
            foreach(BasicFieldType fieldType in Enum.GetValues(typeof(BasicFieldType)))
            {
                List<string> fieldKeys = EZItemManager.ItemFieldKeysOfType(key, fieldType.ToString());                
                foreach(string fieldKey in fieldKeys)
                {
                    currentLinePosition += EZConstants.Indent;
                    DrawSingleField(schemaType, fieldKey, data);
                    shouldDrawSpace = true;
                }
            }
            
            // Draw the custom types
            foreach(string fieldKey in EZItemManager.ItemCustomFieldKeys(key))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    NewLine(0.5f);
                    didDrawSpaceForSection = true;
                }
                
                currentLinePosition += EZConstants.Indent;
                DrawSingleField(schemaType, fieldKey, data);
                shouldDrawSpace = true;
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
                        NewLine(0.5f);
                        didDrawSpaceForSection = true;
                    }

                    currentLinePosition += EZConstants.Indent;
                    DrawListField(schemaType, fieldKey, data);
                    shouldDrawSpace = true;
                }
            }
            didDrawSpaceForSection = false;
            
            // Draw the custom lists
            foreach(string fieldKey in EZItemManager.ItemCustomListFields(key))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    NewLine(0.5f);
                    didDrawSpaceForSection = true;
                }

                currentLinePosition += EZConstants.Indent;
                DrawListField(schemaType, fieldKey, data);
                shouldDrawSpace = true;
            }

            NewLine(0.5f);

            float width = 50;
            if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Delete"))
                deletedItems.Add(key);

            NewLine(0.75f);

            GUI.Box(new Rect(currentLinePosition, MiddleOfLine(), FullSeparatorWidth(), 1), "");

            NewLine();
        }
    }

    void DrawSingleField(string schemaKey, string fieldKey, Dictionary<string, object> itemData)
    {
        string fieldType = itemData[fieldKey].ToString();
        BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
        if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
        {
            fieldTypeEnum = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), fieldType);
            if (!fieldTypeEnum.Equals(BasicFieldType.Vector2) && 
                !fieldTypeEnum.Equals(BasicFieldType.Vector3) && 
                !fieldTypeEnum.Equals(BasicFieldType.Vector4))
                fieldType = fieldType.ToLower();
        }

        float width = 120;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), fieldType);
        currentLinePosition += (width + 2);

        width = 100;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), fieldKey);
        currentLinePosition += (width + 2);

        switch(fieldTypeEnum)
        {
            case BasicFieldType.Bool:
            {
                DrawBool(fieldKey, itemData, "Value:");
                NewLine();
                break;
            }
            case BasicFieldType.Int:
            {
                DrawInt(fieldKey, itemData, "Value:");
                NewLine();
                break;
            }
            case BasicFieldType.Float:
            {
                DrawFloat(fieldKey, itemData, "Value:");
                NewLine();
                break;
            }
            case BasicFieldType.String:
            {
                DrawString(fieldKey, itemData, "Value:");
                NewLine();
                break;
            }
            case BasicFieldType.Vector2:
            {
                DrawVector2(fieldKey, itemData, "Values:");
                NewLine(EZConstants.VectorFieldBuffer+1);
                break;
            }
            case BasicFieldType.Vector3:
            {
                DrawVector3(fieldKey, itemData, "Values:");
                NewLine(EZConstants.VectorFieldBuffer+1);
                break;
            }
            case BasicFieldType.Vector4:
            {
                DrawVector4(fieldKey, itemData, "Values:");
                NewLine(EZConstants.VectorFieldBuffer+1);
                break;
            }
                
            default:
            {
                List<string> itemKeys = GetPossibleCustomValues(schemaKey, fieldType);
                DrawCustom(fieldKey, itemData, true, itemKeys);
                NewLine();
                break;
            }
        }
    }

    void DrawListField(string schemaKey, string fieldKey, Dictionary<string, object> itemData)
    {
        try
        {
            string foldoutKey = string.Format(EZConstants.MetaDataFormat, schemaKey, fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);
            
            string fieldType = itemData[fieldKey].ToString();
            BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
            if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
            {
                fieldTypeEnum = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), fieldType);
                if (!fieldTypeEnum.Equals(BasicFieldType.Vector2) && 
                    !fieldTypeEnum.Equals(BasicFieldType.Vector3) && 
                    !fieldTypeEnum.Equals(BasicFieldType.Vector4))
                    fieldType = fieldType.ToLower();
            }

            float width = 120;
            newFoldoutState = EditorGUI.Foldout(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), currentFoldoutState, string.Format("List<{0}>", fieldType));
            currentLinePosition += (width + 2);

            width = 100;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), fieldKey);
            currentLinePosition += (width + 2);

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

            width = 40;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Count:");
            currentLinePosition += (width + 2);

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

            width = 40;
            newListCount = EditorGUI.IntField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), newListCount);
            currentLinePosition += (width + 4);

            width = 50;
            newListCountDict[listCountKey] = newListCount;
            if (newListCount != list.Count && GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Resize"))            
            {
                ResizeList(list, newListCount, 0);
                newListCountDict[listCountKey] = newListCount;
                currentLinePosition += (width + 2);
            }

            NewLine();

            if (newFoldoutState)
            {
                for (int i = 0; i < list.Count; i++) 
                {
                   currentLinePosition += EZConstants.Indent*2;
                    
                    switch (fieldTypeEnum) {
                        case BasicFieldType.Bool:
                        {
                            DrawListBool(i, Convert.ToBoolean(list[i]), list);
                            NewLine();
                            break;
                        }
                        case BasicFieldType.Int:
                        {
                            DrawListInt(i, Convert.ToInt32(list[i]), list);
                            NewLine();
                            break;
                        }
                        case BasicFieldType.Float:
                        {
                            DrawListFloat(i, Convert.ToSingle(list[i]), list);
                            NewLine();
                            break;
                        }
                        case BasicFieldType.String:
                        {
                            DrawListString(i, list[i] as string, list);
                            NewLine();
                            break;
                        }
                        case BasicFieldType.Vector2:
                        {
                            DrawListVector2(i, list[i] as Dictionary<string, object>, list);
                            NewLine(EZConstants.VectorFieldBuffer+1);
                            break;
                        }
                        case BasicFieldType.Vector3:
                        {
                            DrawListVector3(i, list[i] as Dictionary<string, object>, list);
                            NewLine(EZConstants.VectorFieldBuffer+1);
                            break;
                        }
                        case BasicFieldType.Vector4:
                        {
                            DrawListVector4(i, list[i] as Dictionary<string, object>, list);
                            NewLine(EZConstants.VectorFieldBuffer+1);
                            break; 
                        }
                        default:
                        {
                            List<string> itemKeys = GetPossibleCustomValues(schemaKey, fieldType);
                            DrawListCustom(i, list[i] as string, list, true, itemKeys);
                            NewLine();
                            break;
                        }
                    }
                }
            }
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
