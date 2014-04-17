using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using EZExtensionMethods;

[Flags]
public enum BasicFieldType
{
    Undefined = 0,
    Bool = 1,
    Int = 2,
    Float = 4,
    String = 8,
    Vector2 = 16,
    Vector3 = 32,
    Vector4 = 64
}

public class EZItemManager
{
    #region Item Dictionary
    public static string ItemFilePath 
    {
        get
        {
            return Application.dataPath + "/" + EditorPrefs.GetString(EZConstants.CreateDataFileKey, EZConstants.CreateDataFile);
        }
    }
    private static Dictionary<string, Dictionary<string, object>> _allItems;
    public static Dictionary<string, Dictionary<string, object>> AllItems
    { 
        private set
        {
            _allItems = value;
        }

        get
        {
            if (_allItems == null)
                _allItems = new Dictionary<string, Dictionary<string, object>>();
            return _allItems;
        }
    }
    #endregion

    #region Schema Dictionary
    public static string SchemaFilePath
    {
        get
        {
            return Application.dataPath + "/" + EditorPrefs.GetString(EZConstants.DefineDataFileKey, EZConstants.DefineDataFile);
        }
    }
    private static Dictionary<string, Dictionary<string, object>> _schema;
    public static Dictionary<string, Dictionary<string, object>> AllSchemas
    {
        private set
        {
            _schema = value;
        }

        get
        { 
            if (_schema == null) 
                _schema = new Dictionary<string, Dictionary<string, object>>();
            return _schema;
        } 
    }
    #endregion

    #region Lists for sorting and lookups
    // Key: field name, List: contains the schema keys that contain that field name
    private static Dictionary<string, List<string>> _listByFieldName;
    private static Dictionary<string, List<string>> ListByFieldName
    {
        set { _listByFieldName = value; }
        get
        {
            if (_listByFieldName == null)
                _listByFieldName = new Dictionary<string, List<string>>();
            return _listByFieldName;
        }
    }

#if UNITY_EDITOR
    public static string GetSchemaForItem(string itemKey)
    {
        string schema = "";
        Dictionary<string, object> itemData;
        if (AllItems.TryGetValue(itemKey, out itemData))
        {
            object temp;
            if (itemData.TryGetValue(EZConstants.SchemaKey, out temp))
                schema = temp as string;
        }

        return schema;
    }

    public static List<string> ItemFieldKeysOfType(string itemKey, string fieldType)
    {
        return FieldKeysOfType(itemKey, fieldType, AllItems);
    }

    public static List<string> ItemCustomFieldKeys(string itemKey)
    {
        return CustomFieldKeys(itemKey, AllItems);
    }

    public static List<string> SchemaFieldKeysOfType(string schemaKey, string fieldType)
    {
        return FieldKeysOfType(schemaKey, fieldType, AllSchemas);
    }

    public static List<string> SchemaCustomFieldKeys(string schemaKey)
    {
        return CustomFieldKeys(schemaKey, AllSchemas);
    }

    private static List<string> FieldKeysOfType(string key, string fieldType, Dictionary<string, Dictionary<string, object>> dict, bool onlyLists = false)
    {
        List<string> fieldKeys = new List<string>();
        Dictionary<string, object> data;

        if (dict.TryGetValue(key, out data))
        {
            foreach(KeyValuePair<string, object> field in data)
            {
                if (field.Key.StartsWith(EZConstants.IsListPrefix) ||
                    field.Key.StartsWith(EZConstants.ValuePrefix) ||
                    field.Key.StartsWith(EZConstants.SchemaKey))
                    continue;

                if (field.Value.ToString().ToLower().Equals(fieldType.ToLower()) && (data.ContainsKey(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, field.Key)) == onlyLists))
                    fieldKeys.Add(field.Key);
            }
        }

        return fieldKeys;
    }

    private static List<string> CustomFieldKeys(string key, Dictionary<string, Dictionary<string, object>> dict, bool onlyLists = false)
    {
        List<string> fieldKeys = new List<string>();
        Dictionary<string, object> data;
        
        if (dict.TryGetValue(key, out data))
        {
            foreach(KeyValuePair<string, object> field in data)
            {
                if (field.Key.StartsWith(EZConstants.IsListPrefix) ||
                    field.Key.StartsWith(EZConstants.ValuePrefix)||
                    field.Key.StartsWith(EZConstants.SchemaKey))
                    continue;

                if (!Enum.IsDefined(typeof(BasicFieldType), field.Value) && (data.ContainsKey(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, field.Key)) == onlyLists))
                    fieldKeys.Add(field.Key);
            }
        }

        return fieldKeys;
    }

    public static List<string> ItemListFieldKeys(string itemKey)
    {
        List<string> fieldKeys = new List<string>();
        Dictionary<string, object> data;

        if (AllItems.TryGetValue(itemKey, out data))
        {
            foreach(KeyValuePair<string, object> field in data)
            {
                if (field.Key.StartsWith(EZConstants.IsListPrefix))
                    fieldKeys.Add(field.Key.Replace(EZConstants.IsListPrefix+"_", ""));
            }
        }

        return fieldKeys;
    }

    public static List<string> ItemListFieldKeysOfType(string itemKey, string fieldType)
    {
        return ListFieldKeysOfType(itemKey, fieldType, AllItems);
    }

    public static List<string> SchemaListFieldKeysOfType(string schemaKey, string fieldType)
    {
        return ListFieldKeysOfType(schemaKey, fieldType, AllSchemas);
    }

    public static List<string> ItemCustomListFields(string itemKey)
    {
        return ListCustomFieldKeys(itemKey, AllItems);
    }

    public static List<string> SchemaCustomListFields(string schemaKey)
    {
        return ListCustomFieldKeys(schemaKey, AllSchemas);
    }

    private static List<string> ListFieldKeysOfType(string key, string fieldType, Dictionary<string, Dictionary<string, object>> dict)
    {
        return FieldKeysOfType(key, fieldType, dict, true);
    }

    private static List<string> ListCustomFieldKeys(string key, Dictionary<string, Dictionary<string, object>> dict)
    {
        return CustomFieldKeys(key, dict, true);
    }
#endif
    #endregion

    #region Save/Load Methods
    public static void SaveItems()
    {
        try
        {
            string rawJson = Json.Serialize(AllItems);
            string prettyJson = JsonHelper.FormatJson(rawJson);

            File.WriteAllText(ItemFilePath, prettyJson);
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void SaveSchemas()
    {
        try
        {
            string rawJson = Json.Serialize(AllSchemas);
            string prettyJson = JsonHelper.FormatJson(rawJson);

            File.WriteAllText(SchemaFilePath, prettyJson);
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void LoadItems()
    {
        try
        {
            CreateFileIfMissing(ItemFilePath);

            string json = File.ReadAllText(ItemFilePath);
            Dictionary<string, object> data = Json.Deserialize(json) as Dictionary<string, object>;

            AllItems.Clear();

            foreach(KeyValuePair<string, object> pair in data)
            {
                AllItems.Add(pair.Key, pair.Value as Dictionary<string, object>);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void LoadSchemas()
    {
        try
        {
            CreateFileIfMissing(SchemaFilePath);

            string json = File.ReadAllText(SchemaFilePath);
            Dictionary<string, object> data = Json.Deserialize(json) as Dictionary<string, object>;

            AllSchemas.Clear();
            ListByFieldName.Clear();

            foreach(KeyValuePair<string, object> pair in data)
            {
                Dictionary<string, object> schemaData = pair.Value as Dictionary<string, object>;
                AllSchemas.Add(pair.Key, schemaData);

                BuildSortingAndLookupListFor(pair.Key, schemaData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    static void CreateFileIfMissing(string path)
    {
        if (!File.Exists(path))
        {
            StreamWriter writer = File.CreateText(path);
            writer.WriteLine("{}");
            writer.Close();
        }
    }
    #endregion

    #region Add/Remove Methods
    public static bool AddItem(string key, Dictionary<string, object> data)
    {
        return AllItems.TryAddValue(key, data);
    }

    public static void RemoveItem(string key)
    {
        AllItems.Remove(key);
    }

    public static bool AddSchema(string name, Dictionary<string, object> data = null)
    {
        bool result = AllSchemas.TryAddValue(name, data);

        if (result)
            BuildSortingAndLookupListFor(name, data);

        return result;
    }

    public static void AddBasicField(BasicFieldType type, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList)
    {
        AddField(newFieldName, schemaKey, schemaData);
    }

    public static void AddCustomField(string customType, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList)
    {
        AddField(newFieldName, schemaKey, schemaData);
    }

    private static void AddField(string fieldName, string schemaKey, Dictionary<string, object> schemaData)
    {
        // Add the schema key to the listbyfieldname List
        AddFieldToListByFieldName(fieldName, schemaKey);
    }

    private static void AddFieldToListByFieldName(string fieldKey, string schemaKey)
    {
        List<string> schemaKeyList;
        if (ListByFieldName.TryGetValue(fieldKey, out schemaKeyList))
        {
            schemaKeyList.Add(schemaKey);
        }
        else
        {
            schemaKeyList = new List<string>();
            schemaKeyList.Add(schemaKey);
            ListByFieldName.Add(fieldKey, schemaKeyList);
        }
    }

    public static void RemoveField(string schemaKey, Dictionary<string, object> schemaData, string deletedFieldKey)
    {
        // Remove the schema key from the listbyfieldname List
        List<string> schemaKeyList;
        if(ListByFieldName.TryGetValue(deletedFieldKey, out schemaKeyList))
        {
            schemaKeyList.Remove(schemaKey);
            if (schemaKeyList.Count == 0)
                ListByFieldName.Remove(deletedFieldKey);
        }
    }
    #endregion

    #region Filter Methods
    private static void BuildSortingAndLookupListFor(string schemaKey, Dictionary<string, object> schemaData)
    {
        // Parse and add to list by field name
        foreach(KeyValuePair<string, object> field in schemaData)
        {
            // Skip over any metadata
            if (field.Key.StartsWith(EZConstants.ValuePrefix) ||
                field.Key.StartsWith(EZConstants.IsListPrefix))
                continue;

            AddFieldToListByFieldName(field.Key, schemaKey);
        }
    }

    // Returns false if any fields in the given schema start with the given field name
    // Returns true otherwise
    public static bool ShouldFilterByField(string schemaKey, string fieldName)
    {
        List<string> schemaKeyList = null;
        foreach(KeyValuePair<string, List<string>> pair in ListByFieldName)
        {
            if (pair.Key.Contains(fieldName))
            {
                schemaKeyList = pair.Value;
                if (schemaKeyList.Contains(schemaKey))
                    return false;
            }
        }

        return true;
    }
    #endregion
}
