using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

[Flags]
public enum BasicFieldType
{
    Int = 0,
    Float = 1,
    String = 2
}

public class EZItemManager
{
    #region Item Dictionary
    private static string itemFilePath = Application.dataPath + "/ezitems.json";
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

    #region Template Dictionary
    private static string templateFilePath = Application.dataPath + "/eztemplates.json";
    private static Dictionary<string, Dictionary<string, object>> _itemTemplates;
    public static Dictionary<string, Dictionary<string, object>> ItemTemplates
    {
        private set
        {
            _itemTemplates = value;
        }

        get
        { 
            if (_itemTemplates == null) 
                _itemTemplates = new Dictionary<string, Dictionary<string, object>>();
            return _itemTemplates;
        } 
    }
    #endregion

    #region Lists for sorting and lookups
    // Key: field name, List: contains the template keys that contain that field name
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
    #endregion

    #region Save/Load Methods
    public static void SaveItems()
    {
        try
        {
            string rawJson = Json.Serialize(AllItems);
            string prettyJson = JsonHelper.FormatJson(rawJson);

            File.WriteAllText(itemFilePath, prettyJson);
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void SaveTemplates()
    {
        try
        {
            string rawJson = Json.Serialize(ItemTemplates);
            string prettyJson = JsonHelper.FormatJson(rawJson);

            File.WriteAllText(templateFilePath, prettyJson);
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
            string json = File.ReadAllText(itemFilePath);
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

    public static void LoadTemplates()
    {
        try
        {
            string json = File.ReadAllText(templateFilePath);
            Dictionary<string, object> data = Json.Deserialize(json) as Dictionary<string, object>;

            ItemTemplates.Clear();
            ListByFieldName.Clear();

            foreach(KeyValuePair<string, object> pair in data)
            {
                Dictionary<string, object> templateData = pair.Value as Dictionary<string, object>;
                ItemTemplates.Add(pair.Key, templateData);

                BuildSortingAndLookupListFor(pair.Key, templateData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion

    #region Add/Remove Methods
    public static void AddItem(string key, Dictionary<string, object> data)
    {
        AllItems.Add(key, data);
    }

    public static void RemoveItem(string key)
    {
        AllItems.Remove(key);
    }

    public static void AddTemplate(string name, Dictionary<string, object> data = null)
    {
        ItemTemplates.Add(name, data);
        BuildSortingAndLookupListFor(name, data);
    }

    public static void AddBasicField(BasicFieldType type, string templateKey, Dictionary<string, object> templateData, string newFieldName, bool isList)
    {
        AddField(newFieldName, templateKey, templateData);
    }

    public static void AddCustomField(string customType, string templateKey, Dictionary<string, object> templateData, string newFieldName, bool isList)
    {
        AddField(newFieldName, templateKey, templateData);
    }

    private static void AddField(string fieldName, string templateKey, Dictionary<string, object> templateData)
    {
        // Add the template key to the listbyfieldname List
        AddFieldToListByFieldName(fieldName, templateKey);
    }

    private static void AddFieldToListByFieldName(string fieldKey, string templateKey)
    {
        List<string> templateKeyList;
        if (ListByFieldName.TryGetValue(fieldKey, out templateKeyList))
        {
            templateKeyList.Add(templateKey);
        }
        else
        {
            templateKeyList = new List<string>();
            templateKeyList.Add(templateKey);
            ListByFieldName.Add(fieldKey, templateKeyList);
        }
    }

    public static void RemoveField(string templateKey, Dictionary<string, object> templateData, string deletedFieldKey)
    {
        // Remove the template key from the listbyfieldname List
        List<string> templateKeyList;
        if(ListByFieldName.TryGetValue(deletedFieldKey, out templateKeyList))
        {
            templateKeyList.Remove(templateKey);
            if (templateKeyList.Count == 0)
                ListByFieldName.Remove(deletedFieldKey);
        }
    }
    #endregion

    #region Filter Methods
    private static void BuildSortingAndLookupListFor(string templateKey, Dictionary<string, object> templateData)
    {
        // Parse and add to list by field name
        foreach(KeyValuePair<string, object> field in templateData)
        {
            // Skip over any metadata
            if (field.Key.StartsWith(EZConstants.ValuePrefix) ||
                field.Key.StartsWith(EZConstants.IsListPrefix))
                continue;

            AddFieldToListByFieldName(field.Key, templateKey);
        }
    }

    // Returns false if any fields in the given template start with the given field name
    // Returns true otherwise
    public static bool ShouldFilterByField(string templateKey, string fieldName)
    {
        List<string> templateKeyList = null;
        foreach(KeyValuePair<string, List<string>> pair in ListByFieldName)
        {
            if (pair.Key.Contains(fieldName))
            {
                templateKeyList = pair.Value;
                if (templateKeyList.Contains(templateKey))
                    return false;
            }
        }

        return true;
    }
    #endregion
}
