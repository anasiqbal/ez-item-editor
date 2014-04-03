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
    private string newFieldName = "";
    
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

        EditorGUILayout.EndVertical();

        foreach(KeyValuePair<string, object> template in EZItemManager.ItemTemplates)
        {   
            if (DrawFoldout(string.Format("Template: {0}", template.Key), template.Key))
                DrawEntry(template.Key, template.Value);
        }
    }

    private void DrawAddFieldSection(string key, object data)
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Basic Field Type:", GUILayout.Width(100));
        basicFieldTypeSelected = (BasicFieldType)EditorGUILayout.EnumPopup(basicFieldTypeSelected, GUILayout.Width(50));

        EditorGUILayout.LabelField("Field Name:", GUILayout.Width(70));
        newFieldName = EditorGUILayout.TextField(newFieldName);

        if (GUILayout.Button("Add Field"))
            AddBasicField(basicFieldTypeSelected, key, data, newFieldName);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Custom Field Type:", GUILayout.Width(110));

        string[] customTypes = EZItemManager.ItemTemplates.Keys.ToArray();
        customTemplateTypeSelected = EditorGUILayout.Popup(customTemplateTypeSelected, customTypes);

        if (GUILayout.Button("Add Custom Field") && customTypes[customTemplateTypeSelected] != key)
            AddCustomField(customTemplateTypeSelected, key, data);

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
    }

    protected override void DrawEntry(string key, object data)
    {
        List<string> deletedKeys = new List<string>();

        EditorGUILayout.BeginVertical();

        Dictionary<string, object> entry = data as Dictionary<string, object>;

        foreach(string entry_key in entry.Keys.ToArray())
        {
            if (entry_key.StartsWith(EZConstants.ValuePrefix))
                continue;

            string fieldType = entry[entry_key].ToString();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.LabelField(fieldType.ToLower(), GUILayout.Width(50));
            EditorGUILayout.LabelField(entry_key, GUILayout.Width(100));
            EditorGUILayout.LabelField("Default Value:", GUILayout.Width(80));

            switch(fieldType)
            {
                case "Int":
                    DrawInt(entry_key, entry);
                    break;
                case "Float":
                    DrawFloat(entry_key, entry);
                    break;
                case "String":
                    DrawString(entry_key, entry);
                    break;
            }

            if (GUILayout.Button("Delete"))
                deletedKeys.Add(entry_key);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // Remove any fields that were deleted above
        foreach(string entry_key in deletedKeys)
        {
            entry.Remove(entry_key);
            entry.Remove(string.Format("{0}_{1}", EZConstants.ValuePrefix, entry_key));

            Debug.Log("Deleted: "+entry_key);
        }

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

    #region Add Field Methods
    private void AddBasicField(BasicFieldType type, string key, object data, string fieldName)
    {
        Dictionary<string, object> entry = data as Dictionary<string, object>;
        entry.Add(fieldName, type);

        switch(type)
        {
            case BasicFieldType.Int:
            case BasicFieldType.Float:
                entry.Add(string.Format("{0}_{1}", EZConstants.ValuePrefix, fieldName), 0);
                break;

            case BasicFieldType.String:
                entry.Add(string.Format("{0}_{1}", EZConstants.ValuePrefix, fieldName), "");
                break;
        }
    }

    private void AddCustomField(int templateIndex, string key, object data)
    {

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
        foldoutState[key] = true;
    }
    #endregion
}
