using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class EZManagerWindowBase : EditorWindow {

    protected const string rootMenuLocation = "Assets/EZ Item Manager";
    protected Dictionary<string, bool> foldoutState = new Dictionary<string, bool>();
      
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

    protected virtual bool DrawFoldout(string label, string key)
    {
        EditorGUILayout.BeginHorizontal();
        
        bool currentFoldoutState;
        if (!foldoutState.TryGetValue(key, out currentFoldoutState))
            currentFoldoutState = false;
        
        bool newFoldoutState = EditorGUILayout.Foldout(currentFoldoutState, label);
        if (foldoutState.ContainsKey(key))
            foldoutState [key] = newFoldoutState;
        else
            foldoutState.Add(key, newFoldoutState);
        
        EditorGUILayout.EndHorizontal();

        return newFoldoutState;
    }

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

    protected abstract void Load();
    protected abstract void Save();
    protected abstract void Create(object data);
    protected abstract void DrawEntry(string key, object data);
}
