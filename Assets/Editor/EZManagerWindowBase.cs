using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class EZManagerWindowBase : EditorWindow {

    protected const string rootMenuLocation = "Assets/EZ Item Manager";
    protected Dictionary<string, bool> foldoutState = new Dictionary<string, bool>();
    protected bool currentFoldoutAllState = false;

    protected HashSet<string> listFieldFoldoutState = new HashSet<string>();

    protected int newListCount = 0;
      
    protected virtual void OnGUI()
    {
        DrawHeader();
    }

    protected virtual void DrawHeader()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Load"))
            Load();
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Save"))
            Save();
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        GUILayout.Box("", new GUILayoutOption[]
                      {
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1)
        });
        EditorGUILayout.Separator();
    }

    #region Foldout Methods
    protected virtual bool DrawFoldout(string label, string key)
    {
        EditorGUILayout.BeginHorizontal();
        
        bool currentFoldoutState;
        if (!foldoutState.TryGetValue(key, out currentFoldoutState))
            currentFoldoutState = false;
        
        bool newFoldoutState = EditorGUILayout.Foldout(currentFoldoutState, label);
        SetFoldout(newFoldoutState, key);
        
        EditorGUILayout.EndHorizontal();

        return newFoldoutState;
    }

    protected virtual void DrawExpandCollapseAllFoldout(string[] forKeys)
    {
        string label;
        if (currentFoldoutAllState)
            label = "Collapse All";
        else
            label = "Expand All";

        bool newFoldAllState = EditorGUILayout.Foldout(currentFoldoutAllState, label);
        if (newFoldAllState != currentFoldoutAllState) {
            SetAllFoldouts(newFoldAllState, forKeys);
            currentFoldoutAllState = newFoldAllState;
        }
    }

    protected virtual void SetAllFoldouts(bool state, string[] forKeys)
    {
        if (!state)
            foldoutState.Clear();
        else
        {
            foreach(string key in forKeys)
            {
                SetFoldout(state, key);
            }
        }
    }

    protected virtual void SetFoldout(bool state, string forKey)
    {
        if (foldoutState.ContainsKey(forKey))
            foldoutState[forKey] = state;
        else
            foldoutState.Add(forKey, state);
    }
    #endregion

    #region Draw Field Methods
    protected virtual void DrawInt(string fieldName, Dictionary<string, object> data)
    {
        try
        {
            object currentValue;
            int newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            data.TryGetValue(key, out currentValue);
            
            newValue = EditorGUILayout.IntField(Convert.ToInt32(currentValue), GUILayout.Width(50));
            if (newValue != Convert.ToInt32(currentValue))
                data[key] = newValue;
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawListInt(int index, int value, List<object> intList)
    {
        try
        {
            int newValue;
            newValue = EditorGUILayout.IntField(value, GUILayout.Width(50));
            if (value != newValue)
                intList[index] = newValue;
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    
    protected virtual void DrawFloat(string fieldName, Dictionary<string, object> data)
    {
        try
        {
            object currentValue;
            float newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            data.TryGetValue(key, out currentValue);
            
            newValue = EditorGUILayout.FloatField(Convert.ToSingle(currentValue), GUILayout.Width(50));
            if (newValue != Convert.ToSingle(currentValue))
                data[key] = newValue;
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawListFloat(int index, float value, List<object> floatList)
    {
        try
        {
            float newValue;
            newValue = EditorGUILayout.FloatField(value, GUILayout.Width(50));
            if (value != newValue)
                floatList[index] = newValue;
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    
    protected virtual void DrawString(string fieldName, Dictionary<string, object> data)
    {
        try
        {
            object currentValue;
            string newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            data.TryGetValue(key, out currentValue);
            
            newValue = EditorGUILayout.TextField(currentValue as string, GUILayout.Width(100));
            if (newValue != (string)currentValue)
                data[key] = newValue;
            }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawListString(int index, string value, List<object> stringList)
    {
        try
        {
            string newValue;
            newValue = EditorGUILayout.TextField(value, GUILayout.Width(100));
            if (value != newValue)
                stringList[index] = newValue;
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawCustom(string fieldName, Dictionary<string, object> data, bool canEdit, List<string> possibleValues = null)
    {
        try
        {
            object currentValue;
            int newIndex;
            int currentIndex;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);

            data.TryGetValue(key, out currentValue);

            if (canEdit && possibleValues != null)
            {
                currentIndex = possibleValues.IndexOf(currentValue as string);
                newIndex = EditorGUILayout.Popup(currentIndex, possibleValues.ToArray());
                if (newIndex != currentIndex)            
                    data[key] = possibleValues[newIndex];
            }
            else
            {
                EditorGUILayout.LabelField("null", GUILayout.Width(40));
            }
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawListCustom(int index, string value, List<object> customList,  bool canEdit, List<string> possibleValues = null)
    {
        try
        {
            int newIndex;
            int currentIndex;

            if (canEdit && possibleValues != null)
            {
                currentIndex = possibleValues.IndexOf(value);
                newIndex = EditorGUILayout.Popup(currentIndex, possibleValues.ToArray());
                if (newIndex != currentIndex)            
                    customList[index] = possibleValues[newIndex];
            }
            else
            {
                EditorGUILayout.LabelField("null", GUILayout.Width(40));
            }
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion

    protected virtual void ResizeList(List<object> list, int size, object defaultValue)
    {
        // Remove from the end until the size matches what we want
        if (list.Count > size)
            list.RemoveRange(size, list.Count-size);
        else if (list.Count < size)
        {
            // Add entries with the default value until the size is what we want
            for (int i = list.Count; i < size; i++) {
                list.Add(defaultValue);
            }
        }
    }

    #region Abstract methods
    protected abstract void Load();
    protected abstract void Save();
    protected abstract void Create(object data);
    protected abstract void DrawEntry(string key, object data);
    #endregion
}
