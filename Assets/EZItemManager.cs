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
    public static bool ItemsNeedSave;
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
    public static bool SchemasNeedSave;
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

    #region Sorting and Lookup
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

    private static Dictionary<string, List<string>> _itemListBySchema;
    private static Dictionary<string, List<string>> ItemListBySchema
    {
        set { _itemListBySchema = value; }
        get
        {
            if (_itemListBySchema == null)
                _itemListBySchema = new Dictionary<string, List<string>>();
            return _itemListBySchema;
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

    public static List<string> GetItemsOfSchemaType(string schemaType)
    {
        List<string> itemList;
        ItemListBySchema.TryGetValue(schemaType, out itemList);
        return itemList;
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

    private static List<string> GetAllFieldKeys(string key, Dictionary<string, Dictionary<string, object>> dict)
    {
        List<string> allFields = new List<string>();
        Dictionary<string, object> data;        

        if (dict.TryGetValue(key, out data))
        {
            foreach(KeyValuePair<string, object> field in data)
            {
                if (field.Key.StartsWith(EZConstants.IsListPrefix) ||
                    field.Key.StartsWith(EZConstants.ValuePrefix) ||
                    field.Key.StartsWith(EZConstants.SchemaKey))
                    continue;
                
                allFields.Add(field.Key);
            }
        }

        return allFields;
    }

    private static List<string> ListFieldKeysOfType(string key, string fieldType, Dictionary<string, Dictionary<string, object>> dict)
    {
        return FieldKeysOfType(key, fieldType, dict, true);
    }

    private static List<string> ListCustomFieldKeys(string key, Dictionary<string, Dictionary<string, object>> dict)
    {
        return CustomFieldKeys(key, dict, true);
    }

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
        
        // Create empty list for the Item by Schema list
        ItemListBySchema.Add(schemaKey, new List<string>());
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

    private static void AddItemToListBySchema(string itemKey, string schemaKey)
    {
        List<string> itemList;
        if (ItemListBySchema.TryGetValue(schemaKey, out itemList))
        {
            if (!itemList.Contains(itemKey))
                itemList.Add(itemKey);
        }
        else
        {
            itemList = new List<string>();
            itemList.Add(itemKey);
            ItemListBySchema.Add(schemaKey, itemList);
        }
    }
#endif
    #endregion

    #region Save/Load Methods    
    public static void Load()
    {
        LoadSchemas();
        LoadItems();
    }

    public static void Save()
    {
        SaveSchemas();
        SaveItems();
    }

    private static void SaveItems()
    {
        try
        {
            string rawJson = Json.Serialize(AllItems);
            string prettyJson = JsonHelper.FormatJson(rawJson);

            File.WriteAllText(ItemFilePath, prettyJson);

            ItemsNeedSave = false;
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static void SaveSchemas()
    {
        try
        {
            string rawJson = Json.Serialize(AllSchemas);
            string prettyJson = JsonHelper.FormatJson(rawJson);

            File.WriteAllText(SchemaFilePath, prettyJson);

            SchemasNeedSave = false;
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static void LoadItems()
    {
        try
        {
            CreateFileIfMissing(ItemFilePath);

            string json = File.ReadAllText(ItemFilePath);
            Dictionary<string, object> data = Json.Deserialize(json) as Dictionary<string, object>;

            AllItems.Clear();

            foreach(KeyValuePair<string, object> pair in data)
            {
                AddItem(pair.Key, pair.Value as Dictionary<string, object>);
            }

            ItemsNeedSave = false;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static void LoadSchemas()
    {
        try
        {
            CreateFileIfMissing(SchemaFilePath);

            string json = File.ReadAllText(SchemaFilePath);
            Dictionary<string, object> data = Json.Deserialize(json) as Dictionary<string, object>;

            // Clear all schema related lists
            AllSchemas.Clear();
            ListByFieldName.Clear();
            ItemListBySchema.Clear();

            foreach(KeyValuePair<string, object> pair in data)
            {
                Dictionary<string, object> schemaData = pair.Value as Dictionary<string, object>;
                AddSchema(pair.Key, schemaData);
            }

            SchemasNeedSave = false;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static void CreateFileIfMissing(string path)
    {
        if (!File.Exists(path))
        {
            StreamWriter writer = File.CreateText(path);
            writer.WriteLine("{}");
            writer.Close();
        }
    }

    private static bool SchemaExistsForItem(string itemKey, Dictionary<string, object> itemData)
    {
        bool result = false;
        object schemaType;

        if (itemData.TryGetValue(EZConstants.SchemaKey, out schemaType))
        {
            string schemaKey = schemaType as string;
            if (AllSchemas.ContainsKey(schemaKey))
                result = true;
        }

        return result;

    }
    #endregion

    #region Add/Remove Methods
    public static bool AddItem(string key, Dictionary<string, object> data)
    {
        bool result = true;

        if (IsItemNameValid(key) && SchemaExistsForItem(key, data))
            result = AllItems.TryAddValue(key, data);
        else
            result = false;

        if (result)
        {
            AddItemToListBySchema(key, GetSchemaForItem(key));
            ItemsNeedSave = true;
        }

        return result;
    }

    public static void RemoveItem(string key)
    {
        string schemaKey = GetSchemaForItem(key);
        List<string> itemList;

        // Remove from the Item list by schema
        if(ItemListBySchema.TryGetValue(schemaKey, out itemList)) 
        {
            itemList.Remove(key);
        }

        AllItems.Remove(key);

        ItemsNeedSave = true;
    }

    public static bool AddSchema(string name, Dictionary<string, object> data = null)
    {
        bool result = true;

        if (IsSchemaNameValid(name))
            result = AllSchemas.TryAddValue(name, data);
        else
            result = false;

        if (result)
        {
            BuildSortingAndLookupListFor(name, data);
            SchemasNeedSave = true;
        }

        return result;
    }

    public static void RemoveSchema(string key, bool deleteItems = true)
    {
        if (deleteItems)
        {
            // Delete the items with this schema
            List<string> itemList;
            if (ItemListBySchema.TryGetValue(key, out itemList))
            {
                List<string> itemListCopy = new List<string>(itemList);
                foreach(string itemKey in itemListCopy)
                    RemoveItem(itemKey);
            }
            ItemListBySchema.Remove(key);
        }

        // Remove all the fields so the lookup lists get updated
        List<string> allFields = GetAllFieldKeys(key, AllSchemas);
        foreach(string field in allFields)
            RemoveFieldFromSchema(key, field);
        
        AllSchemas.Remove(key);
        SchemasNeedSave = true;
    }
    #endregion

    #region Add/Remove Schema Field Methods
    public static bool AddBasicFieldToSchema(BasicFieldType type, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList = false, object defaultValue = null)
    {
        bool result = true;
        string valueKey = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName);
        
        if (IsFieldNameValid(schemaKey, newFieldName))
            result = schemaData.TryAddValue(newFieldName, type);
        else
            result = false;

        if (result)
        {
            if (isList)
            {
                schemaData.Add(valueKey, new List<object>());
                schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, newFieldName), true);
            }
            else
            {
                schemaData.Add(valueKey, defaultValue);
            }

            AddFieldToListByFieldName(newFieldName, schemaKey);
            AddBasicFieldToItems(type, schemaKey, newFieldName, isList, defaultValue);
        }

        return result;
    }

    public static bool AddCustomFieldToSchema(string customType, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList)
    {
        bool result = true;

        if (IsFieldNameValid(schemaKey, newFieldName))
            result = schemaData.TryAddValue(newFieldName, customType);
        else
            result = false;

        if (result)
        {
            if (isList)
            {
                schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), new List<object>());
                schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, newFieldName), true);
            }
            else
                schemaData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), "null");

            AddFieldToListByFieldName(newFieldName, schemaKey);
            AddCustomFieldToItems(customType, schemaKey, newFieldName, isList);
        }

        return result;
    }

    public static void RemoveFieldFromSchema(string schemaKey, string deletedFieldKey)
    {
        Dictionary<string, object> schemaData;
        if (AllSchemas.TryGetValue(schemaKey, out schemaData))
            RemoveFieldFromSchema(schemaKey, schemaData, deletedFieldKey);
    }

    public static void RemoveFieldFromSchema(string schemaKey, Dictionary<string, object> schemaData, string deletedFieldKey)
    {
        schemaData.Remove(deletedFieldKey);
        schemaData.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, deletedFieldKey));
        schemaData.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, deletedFieldKey));

        // Remove the schema key from the listbyfieldname List
        List<string> schemaKeyList;
        if(ListByFieldName.TryGetValue(deletedFieldKey, out schemaKeyList))
        {
            schemaKeyList.Remove(schemaKey);
            if (schemaKeyList.Count == 0)
                ListByFieldName.Remove(deletedFieldKey);
        }

        RemoveFieldFromItems(schemaKey, deletedFieldKey);
    }
    #endregion

    #region Add/Remove Item Field Methods
    private static void AddBasicFieldToItems(BasicFieldType type, string schemaKey, string newFieldName, bool isList, object defaultValue)
    {
        List<string> itemKeys = GetItemsOfSchemaType(schemaKey);
        Dictionary<string, object> itemData;

        foreach(string itemKey in itemKeys)
        {
            if (AllItems.TryGetValue(itemKey, out itemData))
            {
                itemData.Add(newFieldName, type);

                if (isList)
                {
                    itemData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), new List<object>());
                    itemData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, newFieldName), true);
                }
                else
                {
                    itemData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), defaultValue);
                }

                ItemsNeedSave = true;
            }
        }
    }

    private static void AddCustomFieldToItems(string customType, string schemaKey, string newFieldName, bool isList)
    {
        List<string> itemKeys = GetItemsOfSchemaType(schemaKey);
        Dictionary<string, object> itemData;
        
        foreach(string itemKey in itemKeys)
        {
            if (AllItems.TryGetValue(itemKey, out itemData))
            {
                itemData.Add(newFieldName, customType);
                
                if (isList)
                {
                    itemData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), new List<object>());
                    itemData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, newFieldName), true);
                }
                else
                {
                    itemData.Add(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, newFieldName), "null");
                }
                
                ItemsNeedSave = true;
            }
        }
    }

    private static void RemoveFieldFromItems(string schemaKey, string deleteFieldName)
    {
        List<string> itemKeys = GetItemsOfSchemaType(schemaKey);
        Dictionary<string, object> itemData;

        foreach(string itemKey in itemKeys)
        {
            if (AllItems.TryGetValue(itemKey, out itemData))
            {
                itemData.Remove(deleteFieldName);
                itemData.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, deleteFieldName));
                itemData.Remove(string.Format(EZConstants.MetaDataFormat, EZConstants.IsListPrefix, deleteFieldName));

                ItemsNeedSave = true;
            }
        }
    }
    #endregion

    #region Filter Methods
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

    #region Validation Methods
    public static bool IsSchemaNameValid(string name)
    {
        bool result = true;

        if (AllSchemas.ContainsKey(name) ||
            !ValidateIdentifier.IsValidIdentifier(name))
            result = false;

        return result;
    }    

    public static bool IsFieldNameValid(string schemaKey, string fieldName)
    {
        bool result = true;

        Dictionary<string, object> data;
        if (AllSchemas.TryGetValue(schemaKey, out data))
        {
            if (data.ContainsKey(fieldName) || 
                !ValidateIdentifier.IsValidIdentifier(fieldName))
                result = false;
        }
        else 
            result = false;

        return result;
    }

    public static bool IsItemNameValid(string name)
    {
        return !string.IsNullOrEmpty(name) && !AllItems.ContainsKey(name);
    }
    #endregion
}
