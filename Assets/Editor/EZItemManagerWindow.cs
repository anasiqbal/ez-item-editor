using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EZExtensionMethods;

public class EZItemManagerWindow : EZManagerWindowBase
{
    private const string menuItemLocation = rootMenuLocation + "/EZ Create Data";

    private string newItemName = "";
    private int schemaIndex = 0;

    private int filterSchemaIndex = 0;

    private List<string> deletedItems = new List<string>();
    private Dictionary<string, string> renamedItems = new Dictionary<string, string>();

    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<EZItemManagerWindow>(false, "EZ Create Data");
    }

    [MenuItem(menuItemLocation, true)]
    private static bool showEditorValidator()
    {
        return true;
    }

    #region OnGUI Method
    protected override void OnGUI()
    {
        mainHeaderText = "Create Game Data";
        headerColor = EditorPrefs.GetString(EZConstants.CreateDataColorKey, EZConstants.CreateDataColor);

        base.OnGUI();

        DrawExpandCollapseAllFoldout(EZItemManager.AllItems.Keys.ToArray(), "Item List");

        scrollViewHeight = HeightToBottomOfWindow();
        scrollViewY = TopOfLine();
        verticalScrollbarPosition = GUI.BeginScrollView(new Rect(currentLinePosition, scrollViewY, FullWindowWidth(), scrollViewHeight), 
                                                        verticalScrollbarPosition,
                                                        new Rect(currentLinePosition, scrollViewY, ScrollViewWidth(), CalculateGroupHeightsTotal()));
        foreach (KeyValuePair<string, Dictionary<string, object>> item in EZItemManager.AllItems)
        {
            float currentGroupHeight;
            if (!groupHeights.TryGetValue(item.Key, out currentGroupHeight))
                currentGroupHeight = EZConstants.LineHeight;

            if (IsVisible(currentGroupHeight))
                DrawEntry(item.Key, item.Value);
            else
            {
                NewLine(currentGroupHeight/EZConstants.LineHeight);
            }
        }
        GUI.EndScrollView();
        
        //Remove any items that were deleted
        foreach(string deletedkey in deletedItems)        
            Remove(deletedkey);
        deletedItems.Clear();

        //Rename any items that were renamed
        string error;
        foreach(KeyValuePair<string, string> pair in renamedItems)
        {
            if (!EZItemManager.RenameItem(pair.Key, pair.Value, null, out error))
                EditorUtility.DisplayDialog("Error!", string.Format("Couldn't rename {0} to {1}: {2}", pair.Key, pair.Value, error), "Ok");
        }

        renamedItems.Clear();
    }
    #endregion

    #region Draw Methods
    protected override void DrawCreateSection()
    {
        DrawSubHeader("Create a New Item");
        
        float width = 60;
        GUI.Label(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Schema:");
        currentLinePosition += (width + 2);
        
        width = 100;
        schemaIndex = EditorGUI.Popup(new Rect(currentLinePosition, PopupTop(), width, StandardHeight()), schemaIndex, EZItemManager.SchemaKeyArray);
        currentLinePosition += (width + 6);
        
        width = 65;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Item Name:");
        currentLinePosition += (width + 2);
        
        width = 180;
        newItemName = EditorGUI.TextField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), newItemName);
        currentLinePosition += (width + 2);
        
        width = 100;
        if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Create New Item"))
        {
            List<object> args = new List<object>();
            args.Add(EZItemManager.SchemaKeyArray[schemaIndex]);
            args.Add(newItemName);
            
            if (Create(args))
            {            
                newItemName = "";
                GUI.FocusControl("");
            }
        }

        NewLine();

        DrawSectionSeparator();
    }
    protected override void DrawFilterSection()
    {
        base.DrawFilterSection();

        float width = 200;

        int totalItems = EZItemManager.AllItems.Count;
        string itemText = totalItems != 1 ? "items" : "item";
        if (!string.IsNullOrEmpty(filterText) || 
            (EZItemManager.FilterSchemaKeyArray.IsValidIndex(filterSchemaIndex) && !EZItemManager.FilterSchemaKeyArray[filterSchemaIndex].Equals("_All")))
        {
            string resultText = string.Format("{0} of {1} {2} displayed", NumberOfItemsBeingShown(EZItemManager.AllItems), totalItems, itemText);
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), resultText);
            currentLinePosition += (width + 2);
        }

        NewLine(1.25f);
        
        // Filter dropdown
        GUIContent content = new GUIContent("Show Items Containing Schema:");
        width = labelStyle.CalcSize(content).x;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Show Items Containing Schema:");
        currentLinePosition += (width + 8);

        width = 100;
        filterSchemaIndex = EditorGUI.Popup(new Rect(currentLinePosition, PopupTop(), width, StandardHeight()), filterSchemaIndex, EZItemManager.FilterSchemaKeyArray);

        NewLine();
    }

    protected override void DrawEntry(string key, Dictionary<string, object> data)
    {
        // If we are filtered out, return
        if (ShouldFilter(key, data))
            return;

        float beginningHeight = CurrentHeight();
        string schemaType = "<unknown>";
        object temp;
        
        if (data.TryGetValue(EZConstants.SchemaKey, out temp))
            schemaType = temp as string;

        // Start drawing below
        if (DrawFoldout(schemaType+":", key, key, key, RenameItem))
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
                    DrawListField(schemaType, key, fieldKey, data);
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
                DrawListField(schemaType, key, fieldKey, data);
                shouldDrawSpace = true;
            }

            NewLine(0.5f);

            float width = 50;
            if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Delete"))
                deletedItems.Add(key);

            NewLine();

            DrawSectionSeparator();

            NewLine(0.25f);
        }
        else
        {
            // Collapse any list foldouts as well
            List<string> listKeys = EZItemManager.ItemListFieldKeys(key);
            string foldoutKey;
            foreach(string listKey in listKeys)
            {
                foldoutKey = string.Format(EZConstants.MetaDataFormat, key, listKey);
                listFieldFoldoutState.Remove(foldoutKey);
            }
        }

        float newGroupHeight = CurrentHeight() - beginningHeight;
        float currentGroupHeight;
        groupHeights.TryGetValue(key, out currentGroupHeight);

        // Set the minimum height for the schema type
        if (currentGroupHeight.NearlyEqual(EZConstants.LineHeight) && !newGroupHeight.NearlyEqual(EZConstants.LineHeight))
            SetSchemaHeight(schemaType, newGroupHeight);

        SetGroupHeight(key, newGroupHeight);
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

        width = 120;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), fieldKey.HighlightSubstring(filterText, highlightColor), labelStyle);
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

    void DrawListField(string schemaKey, string itemKey, string fieldKey, Dictionary<string, object> itemData)
    {
        try
        {
            string foldoutKey = string.Format(EZConstants.MetaDataFormat, itemKey, fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);
            object defaultResizeValue = null;
            
            string fieldType = itemData[fieldKey].ToString();
            BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
            if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
            {
                fieldTypeEnum = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), fieldType);
                if (!fieldTypeEnum.Equals(BasicFieldType.Vector2) && 
                    !fieldTypeEnum.Equals(BasicFieldType.Vector3) && 
                    !fieldTypeEnum.Equals(BasicFieldType.Vector4))
                    fieldType = fieldType.ToLower();

                defaultResizeValue = GetDefaultValueForType(fieldTypeEnum);
            }

            float width = 120;
            newFoldoutState = EditorGUI.Foldout(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), currentFoldoutState, string.Format("List<{0}>", fieldType), true);
            currentLinePosition += (width + 2);

            width = 120;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), fieldKey.HighlightSubstring(filterText, highlightColor), labelStyle);
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
            string listCountKey = string.Format(EZConstants.MetaDataFormat, itemKey, fieldKey);
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
                ResizeList(list, newListCount, defaultResizeValue);
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

    #region Filter Methods
    protected override bool ShouldFilter(string itemKey, Dictionary<string, object> itemData)
    {
        if (itemData == null)
            return true;

        string schemaType = "<unknown>";
        object temp;
        
        if (itemData.TryGetValue(EZConstants.SchemaKey, out temp))
            schemaType = temp as string;
        
        // Return if we don't match any of the filter types
        if (EZItemManager.FilterSchemaKeyArray.IsValidIndex(filterSchemaIndex) &&
            !EZItemManager.FilterSchemaKeyArray[filterSchemaIndex].Equals("_All") &&
            !schemaType.Equals(EZItemManager.FilterSchemaKeyArray[filterSchemaIndex]))
            return true;
        
        bool schemaKeyMatch = schemaType.ToLower().Contains(filterText.ToLower());
        bool fieldKeyMatch = !EZItemManager.ShouldFilterByField(schemaType, filterText);
        bool itemKeyMatch = itemKey.ToLower().Contains(filterText.ToLower());
        
        // Return if the schema keys don't contain the filter text or
        // if the schema fields don't contain the filter text
        if (!schemaKeyMatch && !fieldKeyMatch && !itemKeyMatch)
            return true;

        return false;
    }
    #endregion

    #region Load/Save/Create/Remove Item Methods
    protected override void Load()
    {
        base.Load();
        groupHeights.Clear();
    }

    protected override bool Create(object data)
    {
        bool result = true;
        List<object> args = data as List<object>;
        string schemaKey = args[0] as string;
        string itemName = args[1] as string;

        Dictionary<string, object> schemaData = null;       
        if (EZItemManager.AllSchemas.TryGetValue(schemaKey, out schemaData))
        {
            Dictionary<string, object> itemData = new Dictionary<string, object>(schemaData);
            itemData.Add(EZConstants.SchemaKey, schemaKey);

            string error;
            if (EZItemManager.AddItem(itemName, itemData, out error))
            {
                SetFoldout(true, itemName);
                SetNeedToSave(true);
            }
            else
            {
                result = false;
                EditorUtility.DisplayDialog("Error Creating Item!", error, "Ok");
            }
        }
        else
        {
            result = false;
            EditorUtility.DisplayDialog("Error!", "Schema data not found: " + schemaKey, "Ok");
        }

        return result;
    }

    protected override void Remove(string key)
    {
        EZItemManager.RemoveItem(key);
        SetNeedToSave(true);
    }

    protected override bool NeedToSave()
    {
        return EZItemManager.ItemsNeedSave;
    }

    protected override void SetNeedToSave(bool shouldSave)
    {
        EZItemManager.ItemsNeedSave = shouldSave;
    }
    #endregion

    #region Helper Methods
    void SetSchemaHeight(string schemaKey, float groupHeight)
    {
        if (groupHeightBySchema.ContainsKey(schemaKey))
            groupHeightBySchema[schemaKey] = groupHeight;
        else
            groupHeightBySchema.Add(schemaKey, groupHeight);
    }

    protected override float CalculateGroupHeightsTotal()
    {
        float totalHeight = 0;
        float schemaHeight = 0;
        string schema = "";
        
        foreach(KeyValuePair<string, float> pair in groupHeights)
        {
            Dictionary<string, object> itemData;
            EZItemManager.AllItems.TryGetValue(pair.Key, out itemData);
            if (ShouldFilter(pair.Key, itemData))
                continue;

            //Check to see if this item's height has been updated
            //otherwise use the min height for the schema
            if (entryFoldoutState.Contains(pair.Key) && pair.Value.NearlyEqual(EZConstants.LineHeight))
            {
                schema = EZItemManager.GetSchemaForItem(pair.Key);
                groupHeightBySchema.TryGetValue(schema, out schemaHeight);
                totalHeight += schemaHeight;
            }
            else
                totalHeight += pair.Value;
        }
        
        return totalHeight;
    }

    protected override string FilePath()
    {
        return EZItemManager.ItemFilePath;
    }
    #endregion

    #region Rename Methods
    protected bool RenameItem(string oldItemKey, string newItemKey, Dictionary<string, object> data, out string error)
    {
        error = "";
        renamedItems.Add(oldItemKey, newItemKey);
        return true;
    }
    #endregion
}
