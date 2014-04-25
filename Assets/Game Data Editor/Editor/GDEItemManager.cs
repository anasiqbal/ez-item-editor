using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor.MiniJSON;
using GameDataEditor.GDEExtensionMethods;

namespace GameDataEditor
{
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

    public class GDEItemManager
    {
        #region Item Dictionary
        public static bool ItemsNeedSave;
        public static string ItemFilePath 
        {
            get
            {
                return Application.dataPath + "/" + EditorPrefs.GetString(GDEConstants.CreateDataFileKey, GDEConstants.CreateDataFile);
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
                return Application.dataPath + "/" + EditorPrefs.GetString(GDEConstants.DefineDataFileKey, GDEConstants.DefineDataFile);
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
        private static string[] _filterSchemaKeyArray;
        public static string[] FilterSchemaKeyArray
        {
            private set
            {
                _filterSchemaKeyArray = value;
            }

            get
            {
                if (_filterSchemaKeyArray == null)
                    _filterSchemaKeyArray = BuildSchemaFilterKeyArray();

                return _filterSchemaKeyArray;
            }
        }
        private static string[] _schemaKeyArray;
        public static string[] SchemaKeyArray
        {
            private set
            {
                _schemaKeyArray = value;
            }

            get
            {
                if (_schemaKeyArray == null)
                    _schemaKeyArray = BuildSchemaKeyArray();

                return _schemaKeyArray;
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
    
        private static string[] BuildSchemaFilterKeyArray()
        {
            List<string> temp = GDEItemManager.AllSchemas.Keys.ToList();
            temp.Add("_All");
            temp.Sort();
                
            return temp.ToArray();
        }

        private static string[] BuildSchemaKeyArray()
        {
            return GDEItemManager.AllSchemas.Keys.ToArray();
        }

        public static string GetSchemaForItem(string itemKey)
        {
            string schema = "";
            Dictionary<string, object> itemData;
            if (AllItems.TryGetValue(itemKey, out itemData))
            {
                object temp;
                if (itemData.TryGetValue(GDEConstants.SchemaKey, out temp))
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
                    if (field.Key.StartsWith(GDEConstants.IsListPrefix) ||
                        field.Key.StartsWith(GDEConstants.ValuePrefix) ||
                        field.Key.StartsWith(GDEConstants.SchemaKey))
                        continue;

                    if (field.Value.ToString().ToLower().Equals(fieldType.ToLower()) && (data.ContainsKey(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, field.Key)) == onlyLists))
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
                    if (field.Key.StartsWith(GDEConstants.IsListPrefix) ||
                        field.Key.StartsWith(GDEConstants.ValuePrefix)||
                        field.Key.StartsWith(GDEConstants.SchemaKey))
                        continue;

                    if (!Enum.IsDefined(typeof(BasicFieldType), field.Value) && (data.ContainsKey(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, field.Key)) == onlyLists))
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
                    if (field.Key.StartsWith(GDEConstants.IsListPrefix))
                        fieldKeys.Add(field.Key.Replace(GDEConstants.IsListPrefix+"_", ""));
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
                    if (field.Key.StartsWith(GDEConstants.IsListPrefix) ||
                        field.Key.StartsWith(GDEConstants.ValuePrefix) ||
                        field.Key.StartsWith(GDEConstants.SchemaKey))
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
                if (field.Key.StartsWith(GDEConstants.ValuePrefix) ||
                    field.Key.StartsWith(GDEConstants.IsListPrefix))
                    continue;
                
                AddFieldToListByFieldName(field.Key, schemaKey);
            }
            
            // Create empty list for the Item by Schema list
            ItemListBySchema.Add(schemaKey, new List<string>());

            SchemaKeyArray = BuildSchemaKeyArray();
            FilterSchemaKeyArray = BuildSchemaFilterKeyArray();
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

                string error;
                foreach(KeyValuePair<string, object> pair in data)
                {
                    AddItem(pair.Key, pair.Value as Dictionary<string, object>, out error);
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

                string error;
                foreach(KeyValuePair<string, object> pair in data)
                {
                    Dictionary<string, object> schemaData = pair.Value as Dictionary<string, object>;
                    AddSchema(pair.Key, schemaData, out error);
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

            if (itemData.TryGetValue(GDEConstants.SchemaKey, out schemaType))
            {
                string schemaKey = schemaType as string;
                if (AllSchemas.ContainsKey(schemaKey))
                    result = true;
            }

            return result;

        }
        #endregion

        #region Add/Remove Methods
        public static bool AddItem(string key, Dictionary<string, object> data, out string error)
        {
            bool result = true;
            error = "";

            if (IsItemNameValid(key, out error) && SchemaExistsForItem(key, data))
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

        public static bool AddSchema(string name, Dictionary<string, object> data, out string error)
        {
            bool result = true;

            if (IsSchemaNameValid(name, out error))
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
            }
            ItemListBySchema.Remove(key);

            // Remove all the fields so the lookup lists get updated
            List<string> allFields = GetAllFieldKeys(key, AllSchemas);
            foreach(string field in allFields)
                RemoveFieldFromSchema(key, field, deleteItems);
            
            AllSchemas.Remove(key);
            SchemasNeedSave = true;

            SchemaKeyArray = BuildSchemaKeyArray();
            FilterSchemaKeyArray = BuildSchemaFilterKeyArray();
        }
        #endregion

        #region Add/Remove Schema Field Methods
        public static bool AddBasicFieldToSchema(BasicFieldType type, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, out string error, bool isList = false, object defaultValue = null)
        {
            bool result = true;
            string valueKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, newFieldName);
            error = "";

            if (IsFieldNameValid(schemaKey, newFieldName, out error))
                result = schemaData.TryAddValue(newFieldName, type);
            else
                result = false;

            if (result)
            {
                if (isList)
                {
                    schemaData.Add(valueKey, new List<object>());
                    schemaData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, newFieldName), true);
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

        public static bool AddCustomFieldToSchema(string customType, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList, out string error)
        {
            bool result = true;

            if (IsFieldNameValid(schemaKey, newFieldName, out error))
                result = schemaData.TryAddValue(newFieldName, customType);
            else
                result = false;

            if (result)
            {
                if (isList)
                {
                    schemaData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, newFieldName), new List<object>());
                    schemaData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, newFieldName), true);
                }
                else
                    schemaData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, newFieldName), "null");

                AddFieldToListByFieldName(newFieldName, schemaKey);
                AddCustomFieldToItems(customType, schemaKey, newFieldName, isList);
            }

            return result;
        }

        public static void RemoveFieldFromSchema(string schemaKey, string deletedFieldKey, bool deleteFromItem = true)
        {
            Dictionary<string, object> schemaData;
            if (AllSchemas.TryGetValue(schemaKey, out schemaData))
                RemoveFieldFromSchema(schemaKey, schemaData, deletedFieldKey, deleteFromItem);
        }

        public static void RemoveFieldFromSchema(string schemaKey, Dictionary<string, object> schemaData, string deletedFieldKey, bool deleteFromItem = true)
        {
            schemaData.Remove(deletedFieldKey);
            schemaData.Remove(string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, deletedFieldKey));
            schemaData.Remove(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, deletedFieldKey));

            // Remove the schema key from the listbyfieldname List
            List<string> schemaKeyList;
            if(ListByFieldName.TryGetValue(deletedFieldKey, out schemaKeyList))
            {
                schemaKeyList.Remove(schemaKey);
                if (schemaKeyList.Count == 0)
                    ListByFieldName.Remove(deletedFieldKey);
            }

            if (deleteFromItem)
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
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, newFieldName), new List<object>());
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, newFieldName), true);
                    }
                    else
                    {
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, newFieldName), defaultValue);
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
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, newFieldName), new List<object>());
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, newFieldName), true);
                    }
                    else
                    {
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, newFieldName), "null");
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
                    itemData.Remove(string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, deleteFieldName));
                    itemData.Remove(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, deleteFieldName));

                    ItemsNeedSave = true;
                }
            }
        }
        #endregion

        #region Rename Methods
        public static bool RenameSchema(string oldSchemaKey, string newSchemaKey, out string error)
        {
            bool result = true;
            if (IsSchemaNameValid(newSchemaKey, out error))
            {
                Dictionary<string, object> schemaData;
                if (AllSchemas.TryGetValue(oldSchemaKey, out schemaData))
                {
                    List<string> itemsWithSchema = GetItemsOfSchemaType(oldSchemaKey);
                    Dictionary<string, object> schemaDataCopy = schemaData.DeepCopy();

                    // First remove the schema from the dictionary
                    RemoveSchema(oldSchemaKey, false);

                    // Then add the schema data under the new schema key
                    if(AddSchema(newSchemaKey, schemaDataCopy, out error))
                    {
                        List<string> itemBySchemaList;
                        ItemListBySchema.TryGetValue(newSchemaKey, out itemBySchemaList);
                        
                        // Lastly update the schema key on any existing items
                        foreach(string itemKey in itemsWithSchema)
                        {
                            Dictionary<string, object> itemData;
                            if (AllItems.TryGetValue(itemKey, out itemData))                        
                                itemData.TryAddOrUpdateValue(GDEConstants.SchemaKey, newSchemaKey);
                            itemBySchemaList.Add(itemKey);
                        }
                    }
                    else
                    {
                        // Add the schema back under the old key if this step failed
                        AddSchema(oldSchemaKey, schemaDataCopy, out error);
                        result = false;
                    }
                }
                else
                {
                    error = "Failed to read schema data.";
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            SchemasNeedSave |= result;
            ItemsNeedSave |= result;

            return result;
        }

        public static bool RenameItem(string oldItemKey, string newItemKey, Dictionary<string, object> data, out string error)
        {
            bool result = true;
            if (IsItemNameValid(newItemKey, out error))
            {
                Dictionary<string, object> itemData;
                if (AllItems.TryGetValue(oldItemKey, out itemData))
                {
                    Dictionary<string, object> itemDataCopy = itemData.DeepCopy();

                    // First remove the item from the dictionary
                    RemoveItem(oldItemKey);

                    // Then add the item data under the new item key
                    if (!AddItem(newItemKey, itemDataCopy, out error))                
                    {
                        // Add the item back under the old key if this step failed
                        AddItem(oldItemKey, itemDataCopy, out error);
                        result = false;
                    }
                }
                else
                {
                    error = "Failted to read item data.";
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            ItemsNeedSave |= result;

            return result;
        }

        public static bool RenameSchemaField(string oldFieldKey, string newFieldKey, string schemaKey, Dictionary<string, object> schemaData, out string error)
        {
            bool result = true;

            if (!IsFieldNameValid(schemaData, newFieldKey, out error))
            {
                result = false;
            }
            else if (schemaData.ContainsKey(newFieldKey))
            {
                result = false;
                error = "Field name already exists.";
            }
            else
            {
                // Do rename
                RenameField(oldFieldKey, newFieldKey, schemaData);

                // Remove the schema key from the listbyfieldname List
                List<string> schemaKeyList;
                if(ListByFieldName.TryGetValue(oldFieldKey, out schemaKeyList))
                {
                    schemaKeyList.Remove(schemaKey);
                    if (schemaKeyList.Count == 0)
                        ListByFieldName.Remove(oldFieldKey);
                }

                // Add the schema key to the listbyfieldname List under the new field name
                if (ListByFieldName.TryGetValue(newFieldKey, out schemaKeyList))            
                    schemaKeyList.Add(schemaKey);
                else
                {
                    List<string> newListByFieldName = new List<string>(){schemaKey};
                    ListByFieldName.Add(newFieldKey, newListByFieldName);
                }

                // Rename the fields in any existing items with this schema
                List<string> itemKeys = GetItemsOfSchemaType(schemaKey);
                foreach(string itemKey in itemKeys)
                {
                    Dictionary<string, object> itemData;
                    if (AllItems.TryGetValue(itemKey, out itemData))                
                        RenameField(oldFieldKey, newFieldKey, itemData);
                }
            }

            ItemsNeedSave |= result;
            SchemasNeedSave |= result;

            return result;
        }

        private static void RenameField(string oldFieldKey, string newFieldKey, Dictionary<string, object> data)
        {
            object value;
            if (data.TryGetValue(oldFieldKey, out value))
            {
                data.Add(newFieldKey, value);
                data.Remove(oldFieldKey);
            }
            
            string oldKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, oldFieldKey);
            string newKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.ValuePrefix, newFieldKey);
            if (data.TryGetValue(oldKey, out value))
            {
                data.Add(newKey, value);
                data.Remove(oldKey);
            }

            oldKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, oldFieldKey);
            newKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, newFieldKey);
            if (data.TryGetValue(oldKey, out value))
            {
                data.Add(newKey, value);
                data.Remove(oldKey);
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
        public static bool IsSchemaNameValid(string name, out string error)
        {
            bool result = true;
            error = "";

            if (AllSchemas.ContainsKey(name))
            {
                error = "Schema name already exists.";
                result = false;
            }
            else if (!GDEValidateIdentifier.IsValidIdentifier(name))
            {
                error = "Schema name is invalid.";
                result = false;
            }

            return result;
        }    

        public static bool IsFieldNameValid(string schemaKey, string fieldName, out string error)
        {
            bool result = true;
            error = "";

            Dictionary<string, object> data;
            if (AllSchemas.TryGetValue(schemaKey, out data))
            {
                result = IsFieldNameValid(data, fieldName, out error);
            }
            else 
            {
                result = false;
                error = "Error reading item data.";
            }

            return result;
        }

        public static bool IsFieldNameValid(Dictionary<string, object> data, string fieldName, out string error)
        {
            bool result = true;
            error = "";

            if (data.ContainsKey(fieldName))
            {
                error = "Field name already exits.";
                result = false;
            } 
            else if (!GDEValidateIdentifier.IsValidIdentifier(fieldName))
            {
                error = "Field name is invalid.";
                result = false;
            }

            return result;
        }

        public static bool IsItemNameValid(string name, out string error)
        {
            bool result = true;
            error = "";

            if(string.IsNullOrEmpty(name))
            {
                error = "Item name is invalid.";
                result = false;
            }
            else if(AllItems.ContainsKey(name))
            {
                error = "Item name already exists.";
                result = false;
            }

            return result;
        }
        #endregion
    }
}
