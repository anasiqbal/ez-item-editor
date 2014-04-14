﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EZExtensionMethods;

public class EZSchemaManagerWindow : EZManagerWindowBase {

    private const string menuItemLocation = rootMenuLocation + "/EZ Schema Manager";

    private string newSchemaName = "";
    private BasicFieldType basicFieldTypeSelected = BasicFieldType.Int;
    private int customSchemaTypeSelected = 0;

    private Dictionary<string, string> newBasicFieldName = new Dictionary<string, string>();
    private HashSet<string> isBasicList = new HashSet<string>();
    private Dictionary<string, string> newCustomFieldName = new Dictionary<string, string>();
    private HashSet<string> isCustomList = new HashSet<string>();
    
    private List<string> deletedFields = new List<string>();
    
    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<EZSchemaManagerWindow>(false, "EZ Schema Manager");
    }
    
    [MenuItem(menuItemLocation, true)]
    private static bool showEditorValidator()
    {
        return true;
    }

    #region OnGUI Method
    protected override void OnGUI()
    {
        base.OnGUI();

        NewLine();

        float width = 100;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Schema Name:");
        currentLinePosition += (width + 2);

        width = 120;
        newSchemaName = EditorGUI.TextField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), newSchemaName);
        currentLinePosition += (width + 2);

        width = 120;
        if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Create New Schema") && !string.IsNullOrEmpty(newSchemaName))
            Create(newSchemaName);

        NewLine();

        DrawExpandCollapseAllFoldout(EZItemManager.AllSchemas.Keys.ToArray());

        scrollViewHeight = HeightToBottomOfWindow();
        scrollViewY = TopOfLine();
        verticalScrollbarPosition = GUI.BeginScrollView(new Rect(currentLinePosition, scrollViewY, FullWindowWidth(), scrollViewHeight), 
                                                        verticalScrollbarPosition,
                                                        new Rect(currentLinePosition, scrollViewY, ScrollViewWidth(), CalculateGroupHeightsTotal()));
        foreach(KeyValuePair<string, Dictionary<string, object>> schema in EZItemManager.AllSchemas)
        {   
            float currentGroupHeight;
            if (!groupHeights.TryGetValue(schema.Key, out currentGroupHeight))
                currentGroupHeight = EZConstants.LineHeight;
            
            if (IsVisible(currentGroupHeight))
                DrawEntry(schema.Key, schema.Value);
            else
            {
                NewLine(currentGroupHeight/EZConstants.LineHeight);
            }
        }
        GUI.EndScrollView();
    }
    #endregion

    #region DrawAddFieldSection Method
    private void DrawAddFieldSection(string schemaKey, Dictionary<string, object> schemaData)
    {
        currentLinePosition += EZConstants.Indent;

        // Basic Field Type Group
        float width = 120;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Basic Field Type:");
        currentLinePosition += (width + 2);

        width = 80;
        basicFieldTypeSelected = (BasicFieldType)EditorGUI.EnumPopup(new Rect(currentLinePosition, PopupTop(), width, StandardHeight()), basicFieldTypeSelected);
        currentLinePosition += (width + 6);

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
            AddBasicField(basicFieldTypeSelected, schemaKey, schemaData, newBasicFieldNameText, isBasicListTemp);
            isBasicList.Remove(schemaKey);
            newBasicFieldName.TryAddOrUpdateValue(schemaKey, "");
            newBasicFieldNameText = "";
        }

        NewLine();

        // Custom Field Type Group
        currentLinePosition += EZConstants.Indent;

        width = 120;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Custom Field Type:");
        currentLinePosition += (width + 2);

        List<string> customTypeList = EZItemManager.AllSchemas.Keys.ToList();
        customTypeList.Remove(schemaKey);

        string[] customTypes = customTypeList.ToArray();

        width = 80;
        customSchemaTypeSelected = EditorGUI.Popup(new Rect(currentLinePosition, PopupTop(), width, StandardHeight()), customSchemaTypeSelected, customTypes);
        currentLinePosition += (width + 6);

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
            AddCustomField(customTypes[customSchemaTypeSelected], schemaKey, schemaData, newCustomFieldNameText, isCustomListTemp);
            isCustomList.Remove(schemaKey);
            newCustomFieldName.TryAddOrUpdateValue(schemaKey, "");
            newCustomFieldNameText = "";
        }

        NewLine();
    }
    #endregion

    #region DrawEntry Methods
    protected override void DrawEntry(string schemaKey, Dictionary<string, object> schemaData)
    {
        // If we are filtered out, return
        if (ShouldFilter(schemaKey, schemaData))
            return;

        float beginningHeight = CurrentHeight();

        // Start drawing below
        if (DrawFoldout(string.Format("Schema: {0}", schemaKey), schemaKey))
        {
            bool shouldDrawSpace = false;
            bool didDrawSpaceForSection = false;

            // Draw the basic types
            foreach(BasicFieldType fieldType in Enum.GetValues(typeof(BasicFieldType)))
            {
                List<string> fieldKeys = EZItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString());
                foreach(string fieldKey in fieldKeys)
                {
                    currentLinePosition += EZConstants.Indent;
                    DrawSingleField(fieldKey, schemaData);
                    shouldDrawSpace = true;
                }
            }

            // Draw the custom types
            foreach(string fieldKey in EZItemManager.SchemaCustomFieldKeys(schemaKey))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    NewLine(0.5f);
                    didDrawSpaceForSection = true;
                }

                currentLinePosition += EZConstants.Indent;
                DrawSingleField(fieldKey, schemaData);
                shouldDrawSpace = true;
            }
            didDrawSpaceForSection = false;

            // Draw the lists
            foreach(BasicFieldType fieldType in Enum.GetValues(typeof(BasicFieldType)))
            {
                List<string> fieldKeys = EZItemManager.SchemaListFieldKeysOfType(schemaKey, fieldType.ToString());
                
                foreach(string fieldKey in fieldKeys)
                {
                    if (shouldDrawSpace && !didDrawSpaceForSection)
                    {
                        NewLine(0.5f);
                        didDrawSpaceForSection = true;
                    }

                    currentLinePosition += EZConstants.Indent;
                    DrawListField(schemaKey, schemaData, fieldKey);
                    shouldDrawSpace = true;
                }
            }
            didDrawSpaceForSection = false;

            // Draw the custom lists
            foreach(string fieldKey in EZItemManager.SchemaCustomListFields(schemaKey))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    NewLine(0.5f);
                    didDrawSpaceForSection = true;
                }

                currentLinePosition += EZConstants.Indent;
                DrawListField(schemaKey, schemaData, fieldKey);
                shouldDrawSpace = true;
            }

            // Remove any fields that were deleted above
            foreach(string deletedKey in deletedFields)
            {
                RemoveField(schemaKey, schemaData, deletedKey);
            }
            deletedFields.Clear();

            NewLine();

            DrawAddFieldSection(schemaKey, schemaData);

            GUI.Box(new Rect(currentLinePosition, MiddleOfLine(), FullSeparatorWidth(), 1), "");
            
            NewLine();
        }

        float groupHeight = CurrentHeight() - beginningHeight;
        SetGroupHeight(schemaKey, groupHeight);
    }

    void DrawSingleField(string fieldKey, Dictionary<string, object> schemaData)
    {
        string fieldType = schemaData[fieldKey].ToString();
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
            if (GUI.Button(new Rect(currentLinePosition, MiddleOfLine(), width, StandardHeight()), "Delete"))
                deletedFields.Add(fieldKey);

            NewLine(EZConstants.VectorFieldBuffer+1);
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
            string foldoutKey = string.Format(EZConstants.MetaDataFormat, schemaKey, fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);
            object defaultResizeValue = null;

            BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
            string fieldType = schemaData[fieldKey].ToString();
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

            if (schemaData.TryGetValue(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldKey), out temp))
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
        bool fieldKeyMatch = !EZItemManager.ShouldFilterByField(schemaKey, filterText);
        
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
        
        int totalItems = EZItemManager.AllSchemas.Count;
        string itemText = totalItems != 1 ? "items" : "item";
        if (!string.IsNullOrEmpty(filterText))
        {
            string resultText = string.Format("{0} of {1} {2} displayed", NumberOfItemsBeingShown(EZItemManager.AllSchemas), totalItems, itemText);
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), resultText);
            currentLinePosition += (width + 2);
        }
        else
        {
            string resultText = string.Format("{0} {1} displayed", totalItems, itemText);
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), resultText);
            currentLinePosition += (width + 2);
        }
        
        NewLine();
    }
    #endregion

    #region Add/Remove Field Methods
    private void AddBasicField(BasicFieldType type, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList)
    {
        string valueKey = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName);
        schemaData.Add(newFieldName, type);
        object defaultValue = GetDefaultValueForType(type);

        if (isList)
        {
            schemaData.Add(valueKey, new List<object>());
            schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, newFieldName), true);
        }
        else
        {
            schemaData.Add(valueKey, defaultValue);
        }

        // Let the manager know we added a field
        EZItemManager.AddBasicField(type, schemaKey, schemaData, newFieldName, isList);
    }

    private void AddCustomField(string customType, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList)
    {
        schemaData.Add(newFieldName, customType);

        if (isList)
        {
            schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), new List<object>());
            schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, newFieldName), true);
        }
        else
            schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), "null");

        // Let the manager know we added a field
        EZItemManager.AddCustomField(customType, schemaKey, schemaData, newFieldName, isList);
    }

    private void RemoveField(string schemaKey, Dictionary<string, object> schemaData, string deletedFieldKey)
    {
        schemaData.Remove(deletedFieldKey);
        schemaData.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, deletedFieldKey));
        schemaData.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, deletedFieldKey));
        newListCountDict.Remove(string.Format(EZConstants.MetaDataFormat, schemaKey, deletedFieldKey));

        // Let the manager know we removed a field
        EZItemManager.RemoveField(schemaKey, schemaData, deletedFieldKey);
    }
    #endregion

    #region Load, Save, and Create Schema Methods
    protected override void DrawDataFileLabelForHeader()
    {
        GUIContent filePath = new GUIContent(EZItemManager.schemaFilePath);
        float width = labelStyle.CalcSize(filePath).x + 10;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), filePath);
        currentLinePosition += (width + 2);
    }

    protected override void Load()
    {
        EZItemManager.LoadSchemas();

        groupHeights.Clear();
    }

    protected override void Save()
    {
        EZItemManager.SaveSchemas();
    }

    protected override void Create(object data)
    {
        string key = data as string;
        EZItemManager.AddSchema(key, new Dictionary<string, object>());
        SetFoldout(true, key);
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
    #endregion
}