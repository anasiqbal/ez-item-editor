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
        Vector4 = 64,
        Color = 128
    }

    public class GDEItemManager
    {
        #region Item Dictionary
        private static string _dataFileMD5;
        public static bool ItemsNeedSave;
        public static string DataFilePath 
        {
            get
            {
                return EditorPrefs.GetString(GDEConstants.DataFileKey, GDEConstants.DataFile);
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
        // Basic Field Type string[]
        private static string[] _basicFieldTypeStringArray = null;
        public static string[] BasicFieldTypeStringArray
        {
            private set { _basicFieldTypeStringArray = value; }
            get
            {
                if (_basicFieldTypeStringArray == null)
                    _basicFieldTypeStringArray = BuildBasicTypeStringArray();  

                return _basicFieldTypeStringArray;
            }
        }

        //Basic Field Type List
        private static List<BasicFieldType> _basicFieldTypes = null;
        public static List<BasicFieldType> BasicFieldTypes
        {
            private set { _basicFieldTypes = value; }
            get
            {
                if (_basicFieldTypes == null)
                    _basicFieldTypes = BuildBasicTypeList();

                return _basicFieldTypes;
            }
        }

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

        private static List<BasicFieldType> BuildBasicTypeList()
        {
            List<BasicFieldType> basicTypes = Enum.GetValues(typeof(BasicFieldType)).Cast<BasicFieldType>().ToList();
            basicTypes.Remove(BasicFieldType.Undefined);
            return basicTypes;
        }

        private static string[] BuildBasicTypeStringArray()
        {
            string[] basicTypeArray = new string[BasicFieldTypes.Count];
            for(int index=0; index<basicTypeArray.Length;  index++)
                basicTypeArray[index] = BasicFieldTypes[index].ToString();

            return basicTypeArray;
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

            if (itemList == null)
                itemList = new List<string>();

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
            string fieldName;
            string isListKey;

            if (dict.TryGetValue(key, out data))
            {
                foreach(KeyValuePair<string, object> field in data)
                {
                    if (!field.Key.StartsWith(GDEConstants.TypePrefix))
                        continue;

                    fieldName = field.Key.Replace(GDEConstants.TypePrefix, "");
                    isListKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, fieldName);

                    if (field.Value.ToString().ToLower().Equals(fieldType.ToLower()) && (data.ContainsKey(isListKey) == onlyLists))
                        fieldKeys.Add(fieldName);
                }
            }

            return fieldKeys;
        }

        private static List<string> CustomFieldKeys(string key, Dictionary<string, Dictionary<string, object>> dict, bool onlyLists = false)
        {
            List<string> fieldKeys = new List<string>();
            Dictionary<string, object> data;
            string fieldName;
            string isListKey;
            
            if (dict.TryGetValue(key, out data))
            {
                foreach(KeyValuePair<string, object> field in data)
                {
                    if (!field.Key.StartsWith(GDEConstants.TypePrefix))
                        continue;

                    fieldName = field.Key.Replace(GDEConstants.TypePrefix, "");
                    isListKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, fieldName);

                    if (!Enum.IsDefined(typeof(BasicFieldType), field.Value) && (data.ContainsKey(isListKey) == onlyLists))
                        fieldKeys.Add(fieldName);
                }
            }

            return fieldKeys;
        }

        // Returns a list of item keys that are of the List<> type
        public static List<string> ItemListFieldKeys(string itemKey)
        {
            List<string> fieldKeys = new List<string>();
            Dictionary<string, object> data;

            if (AllItems.TryGetValue(itemKey, out data))
            {
                foreach(KeyValuePair<string, object> field in data)
                {
                    if (field.Key.StartsWith(GDEConstants.IsListPrefix))
                        fieldKeys.Add(field.Key.Replace(GDEConstants.IsListPrefix, ""));
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
                        field.Key.StartsWith(GDEConstants.TypePrefix) ||
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

        private static void BuildSortingAndLookupListFor(string schemaKey, Dictionary<string, object> schemaData, bool rebuildArrays = true)
        {
            // Parse and add to list by field name
            foreach(KeyValuePair<string, object> field in schemaData)
            {
                // Skip over any metadata
                if (field.Key.StartsWith(GDEConstants.TypePrefix) ||
                    field.Key.StartsWith(GDEConstants.IsListPrefix))
                    continue;
                
                AddFieldToListByFieldName(field.Key, schemaKey);
            }
            
            // Create empty list for the Item by Schema list
            ItemListBySchema.Add(schemaKey, new List<string>());

            if (rebuildArrays)
            {
                SchemaKeyArray = BuildSchemaKeyArray();
                FilterSchemaKeyArray = BuildSchemaFilterKeyArray();
            }
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
        public static void Load(bool forceLoad = false)
        {
            if (!CreateFileIfMissing(DataFilePath))
                return;

            bool fileChangedOnDisk = FileChangedOnDisk(DataFilePath, _dataFileMD5);

            if (forceLoad || SchemasNeedSave || fileChangedOnDisk)
                LoadSchemas();

            if (forceLoad || ItemsNeedSave || fileChangedOnDisk)
                LoadItems();
        }

        public static void Save()
        {
            try
            {
                Dictionary<string, object> allData = new Dictionary<string, object>();
                foreach(KeyValuePair<string, Dictionary<string, object>> schema in AllSchemas)
                    allData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.SchemaPrefix, schema.Key), schema.Value);

                foreach(KeyValuePair<string, Dictionary<string, object>> item in AllItems)
                    allData.Add(item.Key, item.Value);

                string rawJson = Json.Serialize(allData);
                string prettyJson = JsonHelper.FormatJson(rawJson);
                
                File.WriteAllText(DataFilePath, prettyJson);
                
                ItemsNeedSave = false;
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
                string json = File.ReadAllText(DataFilePath);
                _dataFileMD5 = json.Md5Sum();

                Dictionary<string, object> data = Json.Deserialize(json) as Dictionary<string, object>;

                AllItems.Clear();

                string error;
                foreach(KeyValuePair<string, object> pair in data)
                {
                    if (pair.Key.StartsWith(GDEConstants.SchemaPrefix))
                        continue;

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
                string json = File.ReadAllText(DataFilePath);
                _dataFileMD5 = json.Md5Sum();

                Dictionary<string, object> data = Json.Deserialize(json) as Dictionary<string, object>;

                // Clear all schema related lists
                AllSchemas.Clear();
                ListByFieldName.Clear();
                ItemListBySchema.Clear();
                FilterSchemaKeyArray = null;
                SchemaKeyArray = null;

                string error;
                string schemaName;
                foreach(KeyValuePair<string, object> pair in data)
                {
                    if (!pair.Key.StartsWith(GDEConstants.SchemaPrefix))
                        continue;

                    Dictionary<string, object> schemaData = pair.Value as Dictionary<string, object>;
                    schemaName = pair.Key.Replace(GDEConstants.SchemaPrefix, "");
                    AddSchema(schemaName, schemaData, out error, false);
                }

                SchemaKeyArray = BuildSchemaKeyArray();
                FilterSchemaKeyArray = BuildSchemaFilterKeyArray();

                SchemasNeedSave = false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static bool CreateFileIfMissing(string path)
        {
            bool result = true;

            try
            {
                if (!File.Exists(path))
                {
                    StreamWriter writer = File.CreateText(path);
                    writer.WriteLine("{}");
                    writer.Close();
                }
            }
            catch(DirectoryNotFoundException)
            {
                EditorUtility.DisplayDialog(GDEStrings.ErrorLbl, string.Format(GDEStrings.DirectoryNotFound, path), GDEStrings.OkLbl);
                result = false;
            }

            return result;
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

        private static bool FileChangedOnDisk(string filePath, string cachedMD5)
        {
            bool hasChanged = true;

            try
            {
                string currentMD5 = File.ReadAllText(filePath).Md5Sum();
                hasChanged = cachedMD5 != currentMD5;
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }

            return hasChanged;
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

        public static bool AddSchema(string name, Dictionary<string, object> data, out string error, bool rebuildArrays = true)
        {
            bool result = true;

            if (IsSchemaNameValid(name, out error))
                result = AllSchemas.TryAddValue(name, data);
            else
                result = false;

            if (result)
            {
                BuildSortingAndLookupListFor(name, data, rebuildArrays);
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
            string typeKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, newFieldName);
            error = "";
            bool result = IsFieldNameValid(schemaKey, newFieldName, out error);

            if (result)
            {
                if (isList)
                {
                    result = schemaData.TryAddValue(newFieldName, new List<object>());
                    schemaData.Add(typeKey, type);
                    schemaData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, newFieldName), true);
                }
                else
                {
                    result = schemaData.TryAddValue(newFieldName, defaultValue);
                    schemaData.Add(typeKey, type);
                }

                AddFieldToListByFieldName(newFieldName, schemaKey);
                AddBasicFieldToItems(type, schemaKey, newFieldName, isList, defaultValue);
            }

            return result;
        }

        public static bool AddCustomFieldToSchema(string customType, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList, out string error)
        {
            bool result = IsFieldNameValid(schemaKey, newFieldName, out error);

            if (result)
            {
                if (isList)
                {
                    result = schemaData.TryAddValue(newFieldName, new List<object>());
                    schemaData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, newFieldName), customType);
                    schemaData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, newFieldName), true);
                }
                else
                {
                    result = schemaData.TryAddValue(newFieldName, "null");
                    schemaData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, newFieldName), customType);
                }

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
            schemaData.Remove(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, deletedFieldKey));
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
                    if (isList)
                    {
                        itemData.Add(newFieldName, new List<object>());
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, newFieldName), type);
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, newFieldName), true);
                    }
                    else
                    {
                        itemData.Add(newFieldName, defaultValue);
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, newFieldName), type);
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
                    if (isList)
                    {
                        itemData.Add(newFieldName, new List<object>());
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, newFieldName), customType);
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.IsListPrefix, newFieldName), true);
                    }
                    else
                    {
                        itemData.Add(newFieldName, "null");
                        itemData.Add(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, newFieldName), customType);
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
                    itemData.Remove(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, deleteFieldName));
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
                        
                        // Update the schema key on any existing items
                        foreach(string itemKey in itemsWithSchema)
                        {
                            Dictionary<string, object> itemData;
                            if (AllItems.TryGetValue(itemKey, out itemData))                        
                                itemData.TryAddOrUpdateValue(GDEConstants.SchemaKey, newSchemaKey);
                            itemBySchemaList.Add(itemKey);
                        }

                        // Update any custom fields in schemas that had the old schema name
                        foreach(string curSchemaKey in AllSchemas.Keys)
                        {
                            List<string> fieldsOfSchemaType = SchemaFieldKeysOfType(curSchemaKey, oldSchemaKey);
                            fieldsOfSchemaType.AddRange(SchemaListFieldKeysOfType(curSchemaKey, oldSchemaKey));
                            
                            if (fieldsOfSchemaType.Count > 0)
                            {
                                Dictionary<string, object> curSchemaData;
                                AllSchemas.TryGetValue(curSchemaKey, out curSchemaData);
                                
                                if (curSchemaData == null)
                                    continue;
                                
                                foreach(string schemaFieldKey in fieldsOfSchemaType)
                                    curSchemaData.TryAddOrUpdateValue(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, schemaFieldKey), newSchemaKey);
                            }
                        }

                        // Lastly, update any custom fields that had the old schema name
                        foreach(string curItemKey in AllItems.Keys)
                        {
                            List<string> fieldsOfSchemaType = ItemFieldKeysOfType(curItemKey, oldSchemaKey);
                            fieldsOfSchemaType.AddRange(ItemListFieldKeysOfType(curItemKey, oldSchemaKey));

                            if (fieldsOfSchemaType.Count > 0)
                            {
                                Dictionary<string, object> curItemData;
                                AllItems.TryGetValue(curItemKey, out curItemData);

                                if (curItemData == null)
                                    continue;

                                foreach(string itemFieldKey in fieldsOfSchemaType)
                                    curItemData.TryAddOrUpdateValue(string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, itemFieldKey), newSchemaKey);
                            }
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

                    // Update any items that have a reference to this item
                    string itemSchemaType;
                    itemDataCopy.TryGetString(GDEConstants.SchemaKey, out itemSchemaType);
                    foreach(string curItemKey in AllItems.Keys)
                    {
                        Dictionary<string, object> curItemData;
                        AllItems.TryGetValue(curItemKey, out curItemData);
                        
                        if (curItemData == null)
                            continue;

                        // Update any single field references: ex. custom_type myField = "oldKey"
                        List<string> fieldsOfSchemaType = ItemFieldKeysOfType(curItemKey, itemSchemaType);
                        foreach(string itemFieldKey in fieldsOfSchemaType)
                        {
                            string curItemFieldValue;
                            curItemData.TryGetString(itemFieldKey, out curItemFieldValue);

                            if (!string.IsNullOrEmpty(curItemFieldValue) && curItemFieldValue.Equals(oldItemKey))
                                curItemData.TryAddOrUpdateValue(itemFieldKey, newItemKey);
                        }

                        // Update any references that are part of a list
                        fieldsOfSchemaType = ItemListFieldKeysOfType(curItemKey, itemSchemaType);
                        foreach(string itemFieldKey in fieldsOfSchemaType)
                        {
                            object temp;
                            List<object> valueList = null;
                            curItemData.TryGetValue(itemFieldKey, out temp);
                            valueList = temp as List<object>;

                            if (valueList != null)
                            {
                                List<int> indexes = valueList.AllIndexesOf(oldItemKey);
                                foreach(int index in indexes)
                                    valueList[index] = newItemKey;
                            }
                        }
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
            
            string oldKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, oldFieldKey);
            string newKey = string.Format(GDEConstants.MetaDataFormat, GDEConstants.TypePrefix, newFieldKey);
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
