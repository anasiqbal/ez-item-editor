using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EZTemplateManagerWindow : EZManagerWindowBase {

    private const string menuItemLocation = rootMenuLocation + "/EZ Template Manager";

    private string newTemplateName = "";
    private BasicFieldType basicFieldTypeSelected = BasicFieldType.Int;
    private int customTemplateTypeSelected = 0;

    private Dictionary<string, string> newBasicFieldName = new Dictionary<string, string>();
    private HashSet<string> isBasicList = new HashSet<string>();
    private Dictionary<string, string> newCustomFieldName = new Dictionary<string, string>();
    private HashSet<string> isCustomList = new HashSet<string>();
    
    private List<string> deletedFields = new List<string>();
    
    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<EZTemplateManagerWindow>(false, "EZ Template Manager");
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
        EditorGUILayout.LabelField("Template Name:", GUILayout.Width(100));
        newTemplateName = EditorGUILayout.TextField(newTemplateName);
        if (GUILayout.Button("Create New Template") && !string.IsNullOrEmpty(newTemplateName))
            Create(newTemplateName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        DrawExpandCollapseAllFoldout(EZItemManager.ItemTemplates.Keys.ToArray());

        EditorGUILayout.EndVertical();

        foreach(KeyValuePair<string, Dictionary<string, object>> template in EZItemManager.ItemTemplates)
        {   
            DrawEntry(template.Key, template.Value);
        }
    }
    #endregion

    #region DrawAddFieldSection Method
    private void DrawAddFieldSection(string templateKey, Dictionary<string, object> templateData)
    {
        // Basic Field Type Group
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EZConstants.IndentSize);
       
        EditorGUILayout.LabelField("Basic Field Type:", GUILayout.Width(90));
        basicFieldTypeSelected = (BasicFieldType)EditorGUILayout.EnumPopup(basicFieldTypeSelected, GUILayout.Width(50));

        // Basic field type name field
        string newBasicFieldNameText = "";
        if (!newBasicFieldName.TryGetValue(templateKey, out newBasicFieldNameText))
        {
            newBasicFieldName.Add(templateKey, "");
            newBasicFieldNameText = "";
        }

        EditorGUILayout.LabelField("Field Name:", GUILayout.Width(70));
        newBasicFieldNameText = EditorGUILayout.TextField(newBasicFieldNameText);
        if (!newBasicFieldNameText.Equals(newBasicFieldName[templateKey]))
            newBasicFieldName[templateKey] = newBasicFieldNameText;

        // Basic field type isList checkbox
        bool isBasicListTemp = isBasicList.Contains(templateKey);
        EditorGUILayout.LabelField("Is List:", GUILayout.Width(50));
        isBasicListTemp = EditorGUILayout.Toggle(isBasicListTemp, GUILayout.Width(15));

        if (isBasicListTemp && !isBasicList.Contains(templateKey))
            isBasicList.Add(templateKey);
        else if (!isBasicListTemp && isBasicList.Contains(templateKey))
            isBasicList.Remove(templateKey);

        if (GUILayout.Button("Add Field"))
            AddBasicField(basicFieldTypeSelected, templateKey, templateData, newBasicFieldNameText, isBasicListTemp);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // Custom Field Type Group
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EZConstants.IndentSize);

        EditorGUILayout.LabelField("Custom Field Type:", GUILayout.Width(105));

        List<string> customTypeList = EZItemManager.ItemTemplates.Keys.ToList();
        customTypeList.Remove(templateKey);

        string[] customTypes = customTypeList.ToArray();
        customTemplateTypeSelected = EditorGUILayout.Popup(customTemplateTypeSelected, customTypes, GUILayout.Width(80));

        // Custom field type name field
        string newCustomFieldNameText = "";
        if (!newCustomFieldName.TryGetValue(templateKey, out newCustomFieldNameText))
        {
            newCustomFieldName.Add(templateKey, "");
            newCustomFieldNameText = "";
        }

        EditorGUILayout.LabelField("Field Name:", GUILayout.Width(70));
        newCustomFieldNameText = EditorGUILayout.TextField(newCustomFieldNameText);
        if (!newCustomFieldNameText.Equals(newCustomFieldName[templateKey]))
            newCustomFieldName[templateKey] = newCustomFieldNameText;

        // Custom field type isList checkbox
        bool isCustomListTemp = isCustomList.Contains(templateKey);
        EditorGUILayout.LabelField("Is List:", GUILayout.Width(50));
        isCustomListTemp = EditorGUILayout.Toggle(isCustomListTemp, GUILayout.Width(15));

        if (isCustomListTemp && !isCustomList.Contains(templateKey))
            isCustomList.Add(templateKey);
        else if(!isCustomListTemp && isCustomList.Contains(templateKey))
            isCustomList.Remove(templateKey);

        if (GUILayout.Button("Add Custom Field"))
            AddCustomField(customTypes[customTemplateTypeSelected], templateKey, templateData, newCustomFieldNameText, isCustomListTemp);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region DrawEntry Methods
    protected override void DrawEntry(string templateKey, Dictionary<string, object> templateData)
    {
        bool templateKeyMatch = templateKey.ToLower().Contains(filterText.ToLower());
        bool fieldKeyMatch = !EZItemManager.ShouldFilterByField(templateKey, filterText);

        // Return if the template keys don't contain the filter text or
        // if the template fields don't contain the filter text
        if (!templateKeyMatch && !fieldKeyMatch)
            return;

        // Start drawing below
        if (DrawFoldout(string.Format("Template: {0}", templateKey), templateKey))
        {
            bool shouldDrawSpace = false;
            bool didDrawSpaceForSection = false;
            EditorGUILayout.BeginVertical();

            // Draw the basic types
            foreach(BasicFieldType fieldType in Enum.GetValues(typeof(BasicFieldType)))
            {
                List<string> fieldKeys = EZItemManager.TemplateFieldKeysOfType(templateKey, fieldType.ToString());

                foreach(string fieldKey in fieldKeys)
                {
                    DrawSingleField(fieldKey, templateData);
                    shouldDrawSpace = true;
                }
            }

            // Draw the custom types
            foreach(string fieldKey in EZItemManager.TemplateCustomFieldKeys(templateKey))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    GUILayout.Space(10);
                    didDrawSpaceForSection = true;
                }
                
                shouldDrawSpace = true;
                DrawSingleField(fieldKey, templateData);
            }
            didDrawSpaceForSection = false;

            // Draw the lists
            foreach(BasicFieldType fieldType in Enum.GetValues(typeof(BasicFieldType)))
            {
                List<string> fieldKeys = EZItemManager.TemplateListFieldKeysOfType(templateKey, fieldType.ToString());
                
                foreach(string fieldKey in fieldKeys)
                {
                    if (shouldDrawSpace && !didDrawSpaceForSection)
                    {
                        GUILayout.Space(10);
                        didDrawSpaceForSection = true;
                    }

                    shouldDrawSpace = true;
                    DrawListField(templateKey, templateData, fieldKey);
                }
            }
            didDrawSpaceForSection = false;

            // Draw the custom lists
            foreach(string fieldKey in EZItemManager.TemplateCustomListFields(templateKey))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection)
                {
                    GUILayout.Space(10);
                    didDrawSpaceForSection = true;
                }

                shouldDrawSpace = true;
                DrawListField(templateKey, templateData, fieldKey);
            }

            // Remove any fields that were deleted above
            foreach(string deletedKey in deletedFields)
            {
                RemoveField(templateKey, templateData, deletedKey);
            }
            deletedFields.Clear();

            GUILayout.Space(20);

            DrawAddFieldSection(templateKey, templateData);

            GUILayout.Box("", new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.Height(1)
            });
            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();
        }
    }

    void DrawSingleField(string fieldKey, Dictionary<string, object> templateData)
    {
        string fieldType = templateData[fieldKey].ToString();
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
                DrawInt(fieldKey, templateData);
                break;
            case "float":
                DrawFloat(fieldKey, templateData);
                break;
            case "string":
                DrawString(fieldKey, templateData);
                break;
                
            default:
                DrawCustom(fieldKey, templateData, false);
                break;
        }
        
        if (GUILayout.Button("Delete"))
            deletedFields.Add(fieldKey);
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    void DrawListField(string templateKey, Dictionary<string, object> templateData, string fieldKey)
    {
        try
        {
            string foldoutKey = string.Format(EZConstants.MetaDataFormat, templateKey, fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);

            string fieldType = templateData[fieldKey].ToString();
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

            if (templateData.TryGetValue(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldKey), out temp))
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
    private void AddBasicField(BasicFieldType type, string templateKey, Dictionary<string, object> templateData, string newFieldName, bool isList)
    {
        templateData.Add(newFieldName, type);

        if (isList)
        {
            templateData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), new List<object>());
            templateData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, newFieldName), true);
        }
        else
        {
            switch(type)
            {
                case BasicFieldType.Int:
                {
                    templateData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), 0);
                    break;
                }
                case BasicFieldType.Float:
                {
                    templateData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), 0f);
                    break;
                }
                case BasicFieldType.String:
                {
                    templateData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), "");
                    break;
                }
            }
        }

        // Let the manager know we added a field
        EZItemManager.AddBasicField(type, templateKey, templateData, newFieldName, isList);
    }

    private void AddCustomField(string customType, string templateKey, Dictionary<string, object> templateData, string newFieldName, bool isList)
    {
        templateData.Add(newFieldName, customType);

        if (isList)
        {
            templateData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), new List<object>());
            templateData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, newFieldName), true);
        }
        else
            templateData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), "null");

        // Let the manager know we added a field
        EZItemManager.AddCustomField(customType, templateKey, templateData, newFieldName, isList);
    }

    private void RemoveField(string templateKey, Dictionary<string, object> templateData, string deletedFieldKey)
    {
        templateData.Remove(deletedFieldKey);
        templateData.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, deletedFieldKey));
        templateData.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, deletedFieldKey));
        newListCountDict.Remove(string.Format(EZConstants.MetaDataFormat, templateKey, deletedFieldKey));

        // Let the manager know we removed a field
        EZItemManager.RemoveField(templateKey, templateData, deletedFieldKey);
    }
    #endregion

    #region Load, Save, and Create Template Methods
    protected override void Load()
    {
        EZItemManager.LoadTemplates();
    }

    protected override void Save()
    {
        EZItemManager.SaveTemplates();
    }

    protected override void Create(object data)
    {
        string key = data as string;
        EZItemManager.AddTemplate(key, new Dictionary<string, object>());
        SetFoldout(true, key);
    }
    #endregion
}
