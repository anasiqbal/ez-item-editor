﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class GDESchemaManagerWindow : GDEManagerWindowBase {

    private const string menuItemLocation = rootMenuLocation + "/Define Data";

    private string newSchemaName = "";
    private Dictionary<string, int> basicFieldTypeSelectedDict = new Dictionary<string, int>();
    private Dictionary<string, int> customSchemaTypeSelectedDict = new Dictionary<string, int>();

    private Dictionary<string, string> newBasicFieldName = new Dictionary<string, string>();
    private HashSet<string> isBasicList = new HashSet<string>();
    private Dictionary<string, string> newCustomFieldName = new Dictionary<string, string>();
    private HashSet<string> isCustomList = new HashSet<string>();
    
    private List<string> deletedFields = new List<string>();
    private Dictionary<List<string>, Dictionary<string, object>> renamedFields = new Dictionary<List<string>, Dictionary<string, object>>();

    private List<string> deletedSchemas = new List<string>();
    private Dictionary<string, string> renamedSchemas = new Dictionary<string, string>();
    
    [MenuItem(menuItemLocation, false, menuItemStartPriority+2)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<GDESchemaManagerWindow>(false, "Define Data");
    }

    #region OnGUI/Header Methods
    protected override void OnGUI()
    {
        mainHeaderText = "Define Game Data";
        headerColor = EditorPrefs.GetString(GDEConstants.DefineDataColorKey, GDEConstants.DefineDataColor);

        base.OnGUI();

        DrawExpandCollapseAllFoldout(GDEItemManager.AllSchemas.Keys.ToArray(), "Schema List");

        scrollViewHeight = HeightToBottomOfWindow();
        scrollViewY = TopOfLine();
        verticalScrollbarPosition = GUI.BeginScrollView(new Rect(currentLinePosition, scrollViewY, FullWindowWidth(), scrollViewHeight), 
                                                        verticalScrollbarPosition,
                                                        new Rect(currentLinePosition, scrollViewY, ScrollViewWidth(), CalculateGroupHeightsTotal()));
        foreach(KeyValuePair<string, Dictionary<string, object>> schema in GDEItemManager.AllSchemas)
        {   
            float currentGroupHeight;
            if (!groupHeights.TryGetValue(schema.Key, out currentGroupHeight))
                currentGroupHeight = GDEConstants.LineHeight;
            
            if (IsVisible(currentGroupHeight))
                DrawEntry(schema.Key, schema.Value);
            else
            {
                NewLine(currentGroupHeight/GDEConstants.LineHeight);
            }
        }
        GUI.EndScrollView();

        //Remove any schemas that were deleted
        foreach(string deletedSchemaKey in deletedSchemas)        
            Remove(deletedSchemaKey);
        deletedSchemas.Clear();

        // Rename any schemas that were renamed
        string error;
        foreach(KeyValuePair<string, string> pair in renamedSchemas)
        {
            if (!GDEItemManager.RenameSchema(pair.Key, pair.Value, out error))
                EditorUtility.DisplayDialog("Error!", string.Format("Couldn't rename {0} to {1}: {2}", pair.Key, pair.Value, error), "Ok");
        }
        renamedSchemas.Clear();
    }
    #endregion

    #region Draw Methods
    protected override void DrawCreateSection()
    {
        DrawSubHeader("Create a New Schema");

        float width = 100;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Schema Name:");
        currentLinePosition += (width + 2);
        
        width = 120;
        newSchemaName = EditorGUI.TextField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), newSchemaName);
        currentLinePosition += (width + 2);
        
        width = 120;
        if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Create New Schema"))
        {
            if (Create(newSchemaName))
            {
                newSchemaName = "";
                GUI.FocusControl("");
            }
        }

        NewLine();

        DrawSectionSeparator();
    }

    private void DrawAddFieldSection(string schemaKey, Dictionary<string, object> schemaData)
    {
        currentLinePosition += GDEConstants.Indent;

        string newFieldLabelText = string.Format("<b><color={0}>Add a new field</color></b>", headerColor);
        GUIContent newFieldLabelContent = new GUIContent(newFieldLabelText);
        float width = labelStyle.CalcSize(newFieldLabelContent).x;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), newFieldLabelContent, labelStyle);

        NewLine();

        currentLinePosition += GDEConstants.Indent;

        // ***** Basic Field Type Group ***** //
        width = 120;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Basic Field Type:");
        currentLinePosition += (width + 2);

        // Basic field type selected
        int basicFieldTypeIndex;
        if (!basicFieldTypeSelectedDict.TryGetValue(schemaKey, out basicFieldTypeIndex))
        {
            basicFieldTypeIndex = 0;
            basicFieldTypeSelectedDict.TryAddValue(schemaKey, basicFieldTypeIndex);
        }

        width = 80;
        int newBasicFieldTypeIndex = EditorGUI.Popup(new Rect(currentLinePosition, PopupTop(), width, StandardHeight()), basicFieldTypeIndex, GDEItemManager.BasicFieldTypeStringArray);
        currentLinePosition += (width + 6);

        if (newBasicFieldTypeIndex != basicFieldTypeIndex && GDEItemManager.BasicFieldTypeStringArray.IsValidIndex(newBasicFieldTypeIndex))
        {
            basicFieldTypeIndex = newBasicFieldTypeIndex;
            basicFieldTypeSelectedDict.TryAddOrUpdateValue(schemaKey, basicFieldTypeIndex);
        }


        // Basic field type name field
        string newBasicFieldNameText = "";
        if (!newBasicFieldName.TryGetValue(schemaKey, out newBasicFieldNameText))
        {
            newBasicFieldName.Add(schemaKey, "");
            newBasicFieldNameText = "";
        }

        width = 70;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Field Name:");
        currentLinePosition += (width + 2);

        width = 120;
        newBasicFieldNameText = EditorGUI.TextField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), newBasicFieldNameText);
        currentLinePosition += (width + 6);

        if (!newBasicFieldNameText.Equals(newBasicFieldName[schemaKey]))
            newBasicFieldName[schemaKey] = newBasicFieldNameText;


        // Basic field type isList checkbox
        bool isBasicListTemp = isBasicList.Contains(schemaKey);

        width = 38;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Is List:");
        currentLinePosition += (width + 2);

        width = 15;
        isBasicListTemp = EditorGUI.Toggle(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), isBasicListTemp);
        currentLinePosition += (width + 6);

        if (isBasicListTemp && !isBasicList.Contains(schemaKey))
            isBasicList.Add(schemaKey);
        else if (!isBasicListTemp && isBasicList.Contains(schemaKey))
            isBasicList.Remove(schemaKey);

        width = 65;
        if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Add Field"))
        {
            if (AddBasicField(GDEItemManager.BasicFieldTypes[basicFieldTypeIndex], schemaKey, schemaData, newBasicFieldNameText, isBasicListTemp))
            {
                isBasicList.Remove(schemaKey);
                newBasicFieldName.TryAddOrUpdateValue(schemaKey, "");

                newBasicFieldNameText = "";
                GUI.FocusControl("");
            }
        }

        NewLine();


        // ****** Custom Field Type Group ****** //
        currentLinePosition += GDEConstants.Indent;

        width = 120;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Custom Field Type:");
        currentLinePosition += (width + 2);

        List<string> customTypeList = GDEItemManager.AllSchemas.Keys.ToList();
        customTypeList.Remove(schemaKey);

        string[] customTypes = customTypeList.ToArray();

        int customSchemaTypeIndex;
        if (!customSchemaTypeSelectedDict.TryGetValue(schemaKey, out customSchemaTypeIndex))
        {
            customSchemaTypeIndex = 0;
            customSchemaTypeSelectedDict.TryAddValue(schemaKey, customSchemaTypeIndex);
        }

        // Custom schema type selected
        width = 80;
        int newCustomSchemaTypeSelected = EditorGUI.Popup(new Rect(currentLinePosition, PopupTop(), width, StandardHeight()), customSchemaTypeIndex, customTypes);
        currentLinePosition += (width + 6);

        if (newCustomSchemaTypeSelected != customSchemaTypeIndex && customTypes.IsValidIndex(newCustomSchemaTypeSelected))
        {
            customSchemaTypeIndex = newCustomSchemaTypeSelected;
            customSchemaTypeSelectedDict.TryAddOrUpdateValue(schemaKey, customSchemaTypeIndex);
        }


        // Custom field type name field
        string newCustomFieldNameText = "";
        if (!newCustomFieldName.TryGetValue(schemaKey, out newCustomFieldNameText))
        {
            newCustomFieldName.Add(schemaKey, "");
            newCustomFieldNameText = "";
        }

        width = 70;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Field Name:");
        currentLinePosition += (width + 2);

        width = 120;
        newCustomFieldNameText = EditorGUI.TextField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), newCustomFieldNameText);
        currentLinePosition += (width + 6);

        if (!newCustomFieldNameText.Equals(newCustomFieldName[schemaKey]))
            newCustomFieldName[schemaKey] = newCustomFieldNameText;


        // Custom field type isList checkbox
        bool isCustomListTemp = isCustomList.Contains(schemaKey);

        width = 38;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Is List:");
        currentLinePosition += (width + 2);

        width = 15;
        isCustomListTemp = EditorGUI.Toggle(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), isCustomListTemp);
        currentLinePosition += (width + 6);

        if (isCustomListTemp && !isCustomList.Contains(schemaKey))
            isCustomList.Add(schemaKey);
        else if(!isCustomListTemp && isCustomList.Contains(schemaKey))
            isCustomList.Remove(schemaKey);

        width = 110;
        if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Add Custom Field"))
        {
            if (!customTypes.IsValidIndex(customSchemaTypeIndex) || customTypes.Length.Equals(0))
            {
                EditorUtility.DisplayDialog("Error!", "Invalid custom field type selected.", "Ok");
            }
            else if (AddCustomField(customTypes[customSchemaTypeIndex], schemaKey, schemaData, newCustomFieldNameText, isCustomListTemp))
            {
                isCustomList.Remove(schemaKey);
                newCustomFieldName.TryAddOrUpdateValue(schemaKey, "");
                newCustomFieldNameText = "";
                GUI.FocusControl("");
            }
        }
    }

    protected override void DrawEntry(string schemaKey, Dictionary<string, object> schemaData)
    {
        // If we are filtered out, return
        if (ShouldFilter(schemaKey, schemaData))
            return;

        float beginningHeight = CurrentHeight();

        // Start drawing below
        if (DrawFoldout("Schema: ", schemaKey, schemaKey, schemaKey, RenameSchema))
        {
            bool shouldDrawSpace = false;
            bool didDrawSpaceForSection = false;

            // Draw the basic types
            foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
            {
                List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString());
                foreach(string fieldKey in fieldKeys)
                {
                    currentLinePosition += GDEConstants.Indent;
                    DrawSingleField(schemaKey, fieldKey, schemaData);
                    shouldDrawSpace = true;
                }
            }

            // Draw the custom types
            foreach(string fieldKey in GDEItemManager.SchemaCustomFieldKeys(schemaKey))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    NewLine(0.5f);
                    didDrawSpaceForSection = true;
                }

                currentLinePosition += GDEConstants.Indent;
                DrawSingleField(schemaKey, fieldKey, schemaData);
                shouldDrawSpace = true;
            }
            didDrawSpaceForSection = false;

            // Draw the lists
            foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
            {
                List<string> fieldKeys = GDEItemManager.SchemaListFieldKeysOfType(schemaKey, fieldType.ToString());
                
                foreach(string fieldKey in fieldKeys)
                {
                    if (shouldDrawSpace && !didDrawSpaceForSection)
                    {
                        NewLine(0.5f);
                        didDrawSpaceForSection = true;
                    }

                    currentLinePosition += GDEConstants.Indent;
                    DrawListField(schemaKey, schemaData, fieldKey);
                    shouldDrawSpace = true;
                }
            }
            didDrawSpaceForSection = false;

            // Draw the custom lists
            foreach(string fieldKey in GDEItemManager.SchemaCustomListFields(schemaKey))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    NewLine(0.5f);
                    didDrawSpaceForSection = true;
                }

                currentLinePosition += GDEConstants.Indent;
                DrawListField(schemaKey, schemaData, fieldKey);
                shouldDrawSpace = true;
            }

            // Remove any fields that were deleted above
            foreach(string deletedKey in deletedFields)
            {
                RemoveField(schemaKey, schemaData, deletedKey);
            }
            deletedFields.Clear();

            // Rename any fields that were renamed
            string error;
            string oldFieldKey;
            string newFieldKey;
            foreach(KeyValuePair<List<string>, Dictionary<string, object>> pair in renamedFields)
            {
                oldFieldKey = pair.Key[0];
                newFieldKey = pair.Key[1];
                if (!GDEItemManager.RenameSchemaField(oldFieldKey, newFieldKey, schemaKey, pair.Value, out error))
                    EditorUtility.DisplayDialog("Error!", string.Format("Couldn't rename {0} to {1}: {2}", oldFieldKey, newFieldKey, error), "Ok");
            }
            renamedFields.Clear();

            NewLine();

            DrawAddFieldSection(schemaKey, schemaData);
            
            NewLine(2f);

            float width = 45;
            if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Delete"))
                deletedSchemas.Add(schemaKey);

            NewLine();
            
            DrawSectionSeparator();

            NewLine(0.25f);
        }

        float groupHeight = CurrentHeight() - beginningHeight;
        SetGroupHeight(schemaKey, groupHeight);
    }

    void DrawSingleField(string schemaKey, string fieldKey, Dictionary<string, object> schemaData)
    {
        string fieldType;
        schemaData.TryGetString(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, fieldKey), out fieldType);

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

        string editFieldKey = string.Format(GDEConstants.MetaDataFormat, schemaKey, fieldKey);
        DrawEditableLabel(fieldKey, editFieldKey, RenameSchemaField, schemaData);

        switch(fieldTypeEnum)
        {
            case BasicFieldType.Bool:
                DrawBool(fieldKey, schemaData, "Default Value:");
                break;
            case BasicFieldType.Int:
                DrawInt(fieldKey, schemaData, "Default Value:");
                break;
            case BasicFieldType.Float:
                DrawFloat(fieldKey, schemaData, "Default Value:");
                break;
            case BasicFieldType.String:
                DrawString(fieldKey, schemaData, "Default Value:");
                break;

            case BasicFieldType.Vector2:
                DrawVector2(fieldKey, schemaData, "Default Values:");
                break;
            case BasicFieldType.Vector3:
                DrawVector3(fieldKey, schemaData, "Default Values:");
                break;
            case BasicFieldType.Vector4:
                DrawVector4(fieldKey, schemaData, "Default Values:");
                break;
                
            default:
                DrawCustom(fieldKey, schemaData, false);
                break;
        }

        width = 45;
        if (fieldTypeEnum.Equals(BasicFieldType.Vector2) ||
            fieldTypeEnum.Equals(BasicFieldType.Vector3) ||
            fieldTypeEnum.Equals(BasicFieldType.Vector4))
        {
            if (GUI.Button(new Rect(currentLinePosition, VerticalMiddleOfLine(), width, StandardHeight()), "Delete"))
                deletedFields.Add(fieldKey);

            NewLine(GDEConstants.VectorFieldBuffer+1);
        }
        else
        {
            if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Delete"))
                deletedFields.Add(fieldKey);

            NewLine();
        }
    }

    void DrawListField(string schemaKey, Dictionary<string, object> schemaData, string fieldKey)
    {
        try
        {
            string foldoutKey = string.Format(GDEConstants.MetaDataFormat, schemaKey, fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);
            object defaultResizeValue = null;

            string fieldType;
            schemaData.TryGetString(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, fieldKey), out fieldType);

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
            
            DrawEditableLabel(fieldKey, string.Format(GDEConstants.MetaDataFormat, schemaKey, fieldKey), RenameSchemaField, schemaData);

            if (newFoldoutState != currentFoldoutState)
            {
                if (newFoldoutState)
                    listFieldFoldoutState.Add(foldoutKey);
                else
                    listFieldFoldoutState.Remove(foldoutKey);
            }

            object temp = null;
            List<object> list = null;

            if (schemaData.TryGetValue(fieldKey, out temp))
                list = temp as List<object>;

            width = 40;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Count:");
            currentLinePosition += (width + 2);

            int newListCount;
            string listCountKey = string.Format(GDEConstants.MetaDataFormat, schemaKey, fieldKey);
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
            newListCount = EditorGUI.IntField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), newListCount);
            currentLinePosition += (width + 2);

            newListCountDict[listCountKey] = newListCount;
            width = 50;
            if (newListCount != list.Count)
            {
                if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Resize"))
                    ResizeList(list, newListCount, defaultResizeValue);
                currentLinePosition += (width + 2);
            }
                 
            width = 45;
            if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Delete"))
                deletedFields.Add(fieldKey);

            NewLine();

            if (newFoldoutState)
            {
                for (int i = 0; i < list.Count; i++) 
                {
                    currentLinePosition += GDEConstants.Indent*2;

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
                            NewLine(GDEConstants.VectorFieldBuffer+1);
                            break;
                        }
                        case BasicFieldType.Vector3:
                        {
                            DrawListVector3(i, list[i] as Dictionary<string, object>, list);
                            NewLine(GDEConstants.VectorFieldBuffer+1);
                            break;
                        }
                        case BasicFieldType.Vector4:
                        {
                            DrawListVector4(i, list[i] as Dictionary<string, object>, list);
                            NewLine(GDEConstants.VectorFieldBuffer+1);
                            break;
                        }
                        default:
                        {
                            DrawListCustom(i, list[i] as string, list, false);
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
    #endregion

    #region Filter Methods
    protected override bool ShouldFilter(string schemaKey, Dictionary<string, object> schemaData)
    {
        bool schemaKeyMatch = schemaKey.ToLower().Contains(filterText.ToLower());
        bool fieldKeyMatch = !GDEItemManager.ShouldFilterByField(schemaKey, filterText);
        
        // Return if the schema keys don't contain the filter text or
        // if the schema fields don't contain the filter text
        if (!schemaKeyMatch && !fieldKeyMatch)
            return true;

        return false;
    }

    protected override void DrawFilterSection()
    {
        base.DrawFilterSection();
        
        float width = 200;
        
        int totalItems = GDEItemManager.AllSchemas.Count;
        string itemText = totalItems != 1 ? "items" : "item";
        if (!string.IsNullOrEmpty(filterText))
        {
            string resultText = string.Format("{0} of {1} {2} displayed", NumberOfItemsBeingShown(GDEItemManager.AllSchemas), totalItems, itemText);
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), resultText);
            currentLinePosition += (width + 2);
        }
        
        NewLine();
    }
    #endregion

    #region Add/Remove Field Methods
    private bool AddBasicField(BasicFieldType type, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList)
    {
        bool result = true;
        object defaultValue = GetDefaultValueForType(type);
        string error;

        if (GDEItemManager.AddBasicFieldToSchema(type, schemaKey, schemaData, newFieldName, out error, isList, defaultValue))
            SetNeedToSave(true);
        else
        {
            EditorUtility.DisplayDialog("Error creating field!", error, "Ok");
            result = false;
        }

        return result;
    }

    private bool AddCustomField(string customType, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList)
    {
        bool result = true;
        string error;

        if (GDEItemManager.AddCustomFieldToSchema(customType, schemaKey, schemaData, newFieldName, isList, out error))        
            SetNeedToSave(true);
        else
        {
            EditorUtility.DisplayDialog("Error creating field!", error, "Ok");
            result = false;
        }

        return result;
    }

    private void RemoveField(string schemaKey, Dictionary<string, object> schemaData, string deletedFieldKey)
    {
        newListCountDict.Remove(string.Format(GDEConstants.MetaDataFormat, schemaKey, deletedFieldKey));
        GDEItemManager.RemoveFieldFromSchema(schemaKey, schemaData, deletedFieldKey);

        SetNeedToSave(true);
    }
    #endregion

    #region Load/Save Schema Methods
    protected override void Load()
    {
        GDEItemManager.Load();
        groupHeights.Clear();
    }

    protected override bool NeedToSave()
    {
        return GDEItemManager.SchemasNeedSave;
    }

    protected override void SetNeedToSave(bool shouldSave)
    {
        GDEItemManager.SchemasNeedSave = true;
    }
    #endregion

    #region Create/Remove Schema Methods
    protected override bool Create(object data)
    {
        bool result = true;
        string key = data as string;
        string error;

        result = GDEItemManager.AddSchema(key, new Dictionary<string, object>(), out error);
        if (result)
        {
            SetNeedToSave(true);
            SetFoldout(true, key);
        }
        else
        {
            EditorUtility.DisplayDialog("Error creating Schema!", error, "Ok");
            result = false;
        }

        return result;
    }

    protected override void Remove(string key)
    {
        // Show a warning if we have items using this schema
        List<string> items = GDEItemManager.GetItemsOfSchemaType(key);
        bool shouldDelete = true;

        if (items.Count > 0)
        {
            string itemWord = items.Count == 1 ? "item" : "items";
            shouldDelete = EditorUtility.DisplayDialog(string.Format("{0} {1} will be deleted!", items.Count, itemWord), "Are you sure you want to delete this schema?", "Delete Schema", "Cancel");
        }

        if (shouldDelete)
        {
            GDEItemManager.RemoveSchema(key, true);
            SetNeedToSave(true);
        }
    }
    #endregion

    #region Helper Methods
    protected override float CalculateGroupHeightsTotal()
    {
        float totalHeight = 0;
        foreach(KeyValuePair<string, float> pair in groupHeights)
        {
            if (!ShouldFilter(pair.Key, null))
                totalHeight += pair.Value;
        }
        
        return totalHeight;
    }

    protected override string FilePath()
    {
        return GDEItemManager.SchemaFilePath;
    }
    #endregion

    #region Rename Methods
    protected bool RenameSchema(string oldSchemaKey, string newSchemaKey, Dictionary<string, object> data, out string error)
    {
        error = "";
        renamedSchemas.Add(oldSchemaKey, newSchemaKey);
        return true;
    }

    protected bool RenameSchemaField(string oldFieldKey, string newFieldKey, Dictionary<string, object> schemaData, out string error)
    {
        error = "";
        List<string> fieldKeys = new List<string>(){oldFieldKey, newFieldKey};
        renamedFields.Add(fieldKeys, schemaData);
        return true;
    }
    #endregion
}
