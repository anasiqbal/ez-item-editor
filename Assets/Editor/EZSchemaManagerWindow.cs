using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Schema Name:", GUILayout.Width(100));
        newSchemaName = EditorGUILayout.TextField(newSchemaName);
        if (GUILayout.Button("Create New Schema") && !string.IsNullOrEmpty(newSchemaName))
            Create(newSchemaName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        DrawExpandCollapseAllFoldout(EZItemManager.AllSchemas.Keys.ToArray());

        EditorGUILayout.EndVertical();

        verticalScrollbarPosition = EditorGUILayout.BeginScrollView(verticalScrollbarPosition);
        foreach(KeyValuePair<string, Dictionary<string, object>> schema in EZItemManager.AllSchemas)
        {   
            DrawEntry(schema.Key, schema.Value);
        }
        EditorGUILayout.EndScrollView();
    }
    #endregion

    #region DrawAddFieldSection Method
    private void DrawAddFieldSection(string schemaKey, Dictionary<string, object> schemaData)
    {
        // Basic Field Type Group
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EZConstants.IndentSize);
       
        EditorGUILayout.LabelField("Basic Field Type:", GUILayout.Width(90));
        basicFieldTypeSelected = (BasicFieldType)EditorGUILayout.EnumPopup(basicFieldTypeSelected, GUILayout.Width(50));

        // Basic field type name field
        string newBasicFieldNameText = "";
        if (!newBasicFieldName.TryGetValue(schemaKey, out newBasicFieldNameText))
        {
            newBasicFieldName.Add(schemaKey, "");
            newBasicFieldNameText = "";
        }

        EditorGUILayout.LabelField("Field Name:", GUILayout.Width(70));
        newBasicFieldNameText = EditorGUILayout.TextField(newBasicFieldNameText);
        if (!newBasicFieldNameText.Equals(newBasicFieldName[schemaKey]))
            newBasicFieldName[schemaKey] = newBasicFieldNameText;

        // Basic field type isList checkbox
        bool isBasicListTemp = isBasicList.Contains(schemaKey);
        EditorGUILayout.LabelField("Is List:", GUILayout.Width(50));
        isBasicListTemp = EditorGUILayout.Toggle(isBasicListTemp, GUILayout.Width(15));

        if (isBasicListTemp && !isBasicList.Contains(schemaKey))
            isBasicList.Add(schemaKey);
        else if (!isBasicListTemp && isBasicList.Contains(schemaKey))
            isBasicList.Remove(schemaKey);

        if (GUILayout.Button("Add Field"))
            AddBasicField(basicFieldTypeSelected, schemaKey, schemaData, newBasicFieldNameText, isBasicListTemp);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // Custom Field Type Group
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EZConstants.IndentSize);

        EditorGUILayout.LabelField("Custom Field Type:", GUILayout.Width(105));

        List<string> customTypeList = EZItemManager.AllSchemas.Keys.ToList();
        customTypeList.Remove(schemaKey);

        string[] customTypes = customTypeList.ToArray();
        customSchemaTypeSelected = EditorGUILayout.Popup(customSchemaTypeSelected, customTypes, GUILayout.Width(80));

        // Custom field type name field
        string newCustomFieldNameText = "";
        if (!newCustomFieldName.TryGetValue(schemaKey, out newCustomFieldNameText))
        {
            newCustomFieldName.Add(schemaKey, "");
            newCustomFieldNameText = "";
        }

        EditorGUILayout.LabelField("Field Name:", GUILayout.Width(70));
        newCustomFieldNameText = EditorGUILayout.TextField(newCustomFieldNameText);
        if (!newCustomFieldNameText.Equals(newCustomFieldName[schemaKey]))
            newCustomFieldName[schemaKey] = newCustomFieldNameText;

        // Custom field type isList checkbox
        bool isCustomListTemp = isCustomList.Contains(schemaKey);
        EditorGUILayout.LabelField("Is List:", GUILayout.Width(50));
        isCustomListTemp = EditorGUILayout.Toggle(isCustomListTemp, GUILayout.Width(15));

        if (isCustomListTemp && !isCustomList.Contains(schemaKey))
            isCustomList.Add(schemaKey);
        else if(!isCustomListTemp && isCustomList.Contains(schemaKey))
            isCustomList.Remove(schemaKey);

        if (GUILayout.Button("Add Custom Field"))
            AddCustomField(customTypes[customSchemaTypeSelected], schemaKey, schemaData, newCustomFieldNameText, isCustomListTemp);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region DrawEntry Methods
    protected override void DrawEntry(string schemaKey, Dictionary<string, object> schemaData)
    {
        bool schemaKeyMatch = schemaKey.ToLower().Contains(filterText.ToLower());
        bool fieldKeyMatch = !EZItemManager.ShouldFilterByField(schemaKey, filterText);

        // Return if the schema keys don't contain the filter text or
        // if the schema fields don't contain the filter text
        if (!schemaKeyMatch && !fieldKeyMatch)
            return;

        // Start drawing below
        if (DrawFoldout(string.Format("Schema: {0}", schemaKey), schemaKey))
        {
            bool shouldDrawSpace = false;
            bool didDrawSpaceForSection = false;
            EditorGUILayout.BeginVertical();

            // Draw the basic types
            foreach(BasicFieldType fieldType in Enum.GetValues(typeof(BasicFieldType)))
            {
                List<string> fieldKeys = EZItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString());

                foreach(string fieldKey in fieldKeys)
                {
                    DrawSingleField(fieldKey, schemaData);
                    shouldDrawSpace = true;
                }
            }

            // Draw the custom types
            foreach(string fieldKey in EZItemManager.SchemaCustomFieldKeys(schemaKey))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    GUILayout.Space(10);
                    didDrawSpaceForSection = true;
                }
                
                shouldDrawSpace = true;
                DrawSingleField(fieldKey, schemaData);
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
                        GUILayout.Space(10);
                        didDrawSpaceForSection = true;
                    }

                    shouldDrawSpace = true;
                    DrawListField(schemaKey, schemaData, fieldKey);
                }
            }
            didDrawSpaceForSection = false;

            // Draw the custom lists
            foreach(string fieldKey in EZItemManager.SchemaCustomListFields(schemaKey))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    GUILayout.Space(10);
                    didDrawSpaceForSection = true;
                }

                shouldDrawSpace = true;
                DrawListField(schemaKey, schemaData, fieldKey);
            }

            // Remove any fields that were deleted above
            foreach(string deletedKey in deletedFields)
            {
                RemoveField(schemaKey, schemaData, deletedKey);
            }
            deletedFields.Clear();

            GUILayout.Space(20);

            DrawAddFieldSection(schemaKey, schemaData);

            GUILayout.Box("", new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.Height(1)
            });
            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();
        }
    }

    void DrawSingleField(string fieldKey, Dictionary<string, object> schemaData)
    {
        string fieldType = schemaData[fieldKey].ToString();
        if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
            fieldType = fieldType.ToLower();
        
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Space(EZConstants.IndentSize);
        
        EditorGUILayout.LabelField(fieldType, GUILayout.Width(50));
        EditorGUILayout.LabelField(fieldKey, GUILayout.Width(100));
        EditorGUILayout.LabelField("Default Value:", GUILayout.Width(80));
        
        switch(fieldType)
        {
            case "int":
                DrawInt(fieldKey, schemaData);
                break;
            case "float":
                DrawFloat(fieldKey, schemaData);
                break;
            case "string":
                DrawString(fieldKey, schemaData);
                break;
                
            default:
                DrawCustom(fieldKey, schemaData, false);
                break;
        }
        
        if (GUILayout.Button("Delete"))
            deletedFields.Add(fieldKey);
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    void DrawListField(string schemaKey, Dictionary<string, object> schemaData, string fieldKey)
    {
        try
        {
            string foldoutKey = string.Format(EZConstants.MetaDataFormat, schemaKey, fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);

            string fieldType = schemaData[fieldKey].ToString();
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

            if (schemaData.TryGetValue(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldKey), out temp))
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
            }
                 
            GUILayout.Space(20);
            if (GUILayout.Button("Delete"))
                deletedFields.Add(fieldKey);

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
                            DrawListCustom(i, list[i] as string, list, false);
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
    #endregion

    #region Add/Remove Field Methods
    private void AddBasicField(BasicFieldType type, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList)
    {
        schemaData.Add(newFieldName, type);

        if (isList)
        {
            schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), new List<object>());
            schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, newFieldName), true);
        }
        else
        {
            switch(type)
            {
                case BasicFieldType.Int:
                {
                    schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), 0);
                    break;
                }
                case BasicFieldType.Float:
                {
                    schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), 0f);
                    break;
                }
                case BasicFieldType.String:
                {
                    schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), "");
                    break;
                }
            }
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
    protected override void Load()
    {
        EZItemManager.LoadSchemas();
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
}
