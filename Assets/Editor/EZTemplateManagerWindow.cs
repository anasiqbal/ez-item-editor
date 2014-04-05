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

    private string newBasicFieldName = "";
    private bool isBasicList = false;
    private string newCustomFieldName = "";
    private bool isCustomList = false;
    
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

        foreach(KeyValuePair<string, object> template in EZItemManager.ItemTemplates)
        {   
            DrawEntry(template.Key, template.Value);
        }
    }
    #endregion

    #region DrawAddFieldSection Method
    private void DrawAddFieldSection(string key, object data)
    {
        // Basic Field Type Group
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EZConstants.IndentSize);

        EditorGUILayout.LabelField("Basic Field Type:", GUILayout.Width(90));
        basicFieldTypeSelected = (BasicFieldType)EditorGUILayout.EnumPopup(basicFieldTypeSelected, GUILayout.Width(50));

        EditorGUILayout.LabelField("Field Name:", GUILayout.Width(70));
        newBasicFieldName = EditorGUILayout.TextField(newBasicFieldName);

        EditorGUILayout.LabelField("Is List:", GUILayout.Width(50));
        isBasicList = EditorGUILayout.Toggle(isBasicList, GUILayout.Width(15));

        if (GUILayout.Button("Add Field"))
            AddBasicField(basicFieldTypeSelected, key, data as Dictionary<string, object>, newBasicFieldName, isBasicList);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // Custom Field Type Group
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EZConstants.IndentSize);

        EditorGUILayout.LabelField("Custom Field Type:", GUILayout.Width(105));

        List<string> customTypeList = EZItemManager.ItemTemplates.Keys.ToList();
        customTypeList.Remove(key);

        string[] customTypes = customTypeList.ToArray();
        customTemplateTypeSelected = EditorGUILayout.Popup(customTemplateTypeSelected, customTypes, GUILayout.Width(80));

        EditorGUILayout.LabelField("Field Name:", GUILayout.Width(70));
        newCustomFieldName = EditorGUILayout.TextField(newCustomFieldName);

        EditorGUILayout.LabelField("Is List:", GUILayout.Width(50));
        isCustomList = EditorGUILayout.Toggle(isCustomList, GUILayout.Width(15));

        if (GUILayout.Button("Add Custom Field"))
            AddCustomField(customTypes[customTemplateTypeSelected], key, data as Dictionary<string, object>, newCustomFieldName, isCustomList);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region DrawEntry Methods
    protected override void DrawEntry(string key, object data)
    {
        Dictionary<string, object> entry = data as Dictionary<string, object>;

        // Return if the template keys don't contain the filter text
        if (!key.ToLower().Contains(filterText.ToLower()))
            return;

        // Start drawing below
        if (DrawFoldout(string.Format("Template: {0}", key), key))
        {
            EditorGUILayout.BeginVertical();

            foreach(string entry_key in entry.Keys.ToArray())
            {
                if (entry_key.StartsWith(EZConstants.ValuePrefix) ||
                    entry_key.StartsWith(EZConstants.IsListPrefix))
                    continue;

                if (entry.ContainsKey(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, entry_key)))
                    DrawListField(key, entry_key, entry);
                else
                    DrawSingleField(entry_key, entry);
            }

            // Remove any fields that were deleted above
            foreach(string deletedKey in deletedFields)
            {
                RemoveField(entry, key, deletedKey);
            }
            deletedFields.Clear();

            GUILayout.Space(20);

            DrawAddFieldSection(key, data);

            GUILayout.Box("", new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.Height(1)
            });
            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();
        }
    }

    void DrawSingleField(string entry_key, Dictionary<string, object> entry)
    {
        string fieldType = entry[entry_key].ToString();
        if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
            fieldType = fieldType.ToLower();
        
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Space(EZConstants.IndentSize);
        
        EditorGUILayout.LabelField(fieldType, GUILayout.Width(50));
        EditorGUILayout.LabelField(entry_key, GUILayout.Width(100));
        EditorGUILayout.LabelField("Default Value:", GUILayout.Width(80));
        
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
                DrawCustom(entry_key, entry, false);
                break;
        }
        
        if (GUILayout.Button("Delete"))
            deletedFields.Add(entry_key);
        
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

            int newListCount;
            string listCountKey = string.Format(EZConstants.MetaDataFormat, template_key, entry_key);
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
                deletedFields.Add(entry_key);

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
    private void AddBasicField(BasicFieldType type, string key, Dictionary<string, object> entry, string fieldName, bool isList)
    {
        entry.Add(fieldName, type);

        if (isList)
        {
            entry.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName), new List<object>());
            entry.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, fieldName), true);
        }
        else
        {
            switch(type)
            {
                case BasicFieldType.Int:
                {
                    entry.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName), 0);
                    break;
                }
                case BasicFieldType.Float:
                {
                    entry.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName), 0f);
                    break;
                }
                case BasicFieldType.String:
                {
                    entry.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName), "");
                    break;
                }
            }
        }
    }

    private void AddCustomField(string customType, string key, Dictionary<string, object> entry, string fieldName, bool isList)
    {
        entry.Add(fieldName, customType);

        if (isList)
        {
            entry.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName), new List<object>());
            entry.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, fieldName), true);
        }
        else
            entry.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName), "null");
    }

    private void RemoveField(Dictionary<string, object> entry, string templateKey, string deletedKey)
    {
        entry.Remove(deletedKey);
        entry.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, deletedKey));
        entry.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, deletedKey));
        newListCountDict.Remove(string.Format(EZConstants.MetaDataFormat, templateKey, deletedKey));
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
