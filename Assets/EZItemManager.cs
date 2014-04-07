﻿using UnityEngine;
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
            AllItems = new Dictionary<string, Dictionary<string, object>>();
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
            ItemTemplates = new Dictionary<string, Dictionary<string, object>>();
            foreach(KeyValuePair<string, object> pair in data)
            {
                ItemTemplates.Add(pair.Key, pair.Value as Dictionary<string, object>);
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
        _itemTemplates.Add(name, data);
    }
    #endregion
}
