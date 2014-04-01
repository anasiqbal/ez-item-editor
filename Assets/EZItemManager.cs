using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EZItemManager
{
    private static Dictionary<string, object> _allItems;
    public static Dictionary<string, object> AllItems
    { 
        private set
        {
            _allItems = value;
        }

        get
        {
            if (_allItems == null)
                _allItems = new Dictionary<string, object>();
            return _allItems;
        }
    }

    private static List<string> _itemTemplates;
    public static List<string> ItemTemplates
    {
        private set
        {
            _itemTemplates = value;
        }

        get
        { 
            if (_itemTemplates == null) 
                _itemTemplates = new List<string>();
            return _itemTemplates;
        } 
    }

    public static void Save()
    {

    }

    public static void Load()
    {
        AllItems = new Dictionary<string, object>();
        AddItem("1", "Item details go here....");
        AddItem("2", "Item details go here....");
        AddItem("3", "Item details go here....");

        ItemTemplates = new List<string>();
        AddTemplate("Generic Item");
        AddTemplate("Character");
        AddTemplate("Ability");
    }

    public static void AddItem(string key, object data)
    {
        AllItems.Add(key, data);
    }

    public static void AddTemplate(string name)
    {
        _itemTemplates.Add(name);
    }
}
