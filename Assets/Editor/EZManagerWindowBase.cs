using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class EZManagerWindowBase : EditorWindow {

    protected const string rootMenuLocation = "Assets/EZ Item Manager";
    protected Dictionary<string, bool> foldoutState = new Dictionary<string, bool>();
    protected bool currentFoldoutAllState = false;
      
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
        object currentValue;
        int newValue;
        string key = string.Format("{0}_{1}", EZConstants.ValuePrefix, fieldName);
        
        data.TryGetValue(key, out currentValue);
        
        newValue = EditorGUILayout.IntField(Convert.ToInt32(currentValue), GUILayout.Width(50));
        if (newValue != Convert.ToInt32(currentValue))
            data[key] = newValue;
    }
    
    protected virtual void DrawFloat(string fieldName, Dictionary<string, object> data)
    {
        object currentValue;
        float newValue;
        string key = string.Format("{0}_{1}", EZConstants.ValuePrefix, fieldName);
        
        data.TryGetValue(key, out currentValue);
        
        newValue = EditorGUILayout.FloatField(Convert.ToSingle(currentValue), GUILayout.Width(50));
        if (newValue != Convert.ToSingle(currentValue))
            data[key] = newValue;
    }
    
    protected virtual void DrawString(string fieldName, Dictionary<string, object> data)
    {
        object currentValue;
        string newValue;
        string key = string.Format("{0}_{1}", EZConstants.ValuePrefix, fieldName);
        
        data.TryGetValue(key, out currentValue);
        
        newValue = EditorGUILayout.TextField(currentValue as string, GUILayout.Width(100));
        if (newValue != (string)currentValue)
            data[key] = newValue;
    }
    #endregion

    #region Abstract methods
    protected abstract void Load();
    protected abstract void Save();
    protected abstract void Create(object data);
    protected abstract void DrawEntry(string key, object data);
    #endregion
}
