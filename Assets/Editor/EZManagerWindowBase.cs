using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using EZExtensionMethods;

public abstract class EZManagerWindowBase : EditorWindow {

    protected const string rootMenuLocation = "Assets/EZ Item Manager";

    protected HashSet<string> entryFoldoutState = new HashSet<string>();
    protected HashSet<string> listFieldFoldoutState = new HashSet<string>();
    protected bool currentFoldoutAllState = false;

    protected Dictionary<string, int> newListCountDict = new Dictionary<string, int>();

    protected string filterText = "";

    protected Vector2 verticalScrollbarPosition;
      
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

        GUILayout.Space(15);

        DrawFilterSection();

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
        
        bool currentFoldoutState = entryFoldoutState.Contains(key);
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
            entryFoldoutState.Clear();
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
        if (state)
            entryFoldoutState.Add(forKey);
        else
            entryFoldoutState.Remove(forKey);
    }
    #endregion

    #region Draw Field Methods
    protected virtual void DrawBool(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object currentValue;
            bool newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            data.TryGetValue(key, out currentValue);

            EditorGUILayout.LabelField(label, GUILayout.Width(80));
            newValue = EditorGUILayout.Toggle(Convert.ToBoolean(currentValue), GUILayout.Width(50));
            if (newValue != Convert.ToBoolean(currentValue))
                data[key] = newValue;
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawListBool(int index, bool value, List<object> boolList)
    {
        try
        {
            bool newValue;

            EditorGUILayout.LabelField(string.Format("{0}:", index), GUILayout.Width(20));
            newValue = EditorGUILayout.Toggle(value);

            if (value != newValue)
                boolList[index] = newValue;
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawInt(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object currentValue;
            int newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            data.TryGetValue(key, out currentValue);

            EditorGUILayout.LabelField(label, GUILayout.Width(80));
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

            EditorGUILayout.LabelField(string.Format("{0}:", index), GUILayout.Width(20));
            newValue = EditorGUILayout.IntField(value, GUILayout.Width(50));

            if (value != newValue)
                intList[index] = newValue;
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    
    protected virtual void DrawFloat(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object currentValue;
            float newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            data.TryGetValue(key, out currentValue);

            EditorGUILayout.LabelField(label, GUILayout.Width(80));
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

            EditorGUILayout.LabelField(string.Format("{0}:", index), GUILayout.Width(20));
            newValue = EditorGUILayout.FloatField(value, GUILayout.Width(50));

            if (value != newValue)
                floatList[index] = newValue;
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    
    protected virtual void DrawString(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object currentValue;
            string newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            data.TryGetValue(key, out currentValue);

            EditorGUILayout.LabelField(label, GUILayout.Width(80));
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

            EditorGUILayout.LabelField(string.Format("{0}:", index), GUILayout.Width(20));
            newValue = EditorGUILayout.TextField(value, GUILayout.Width(100));

            if (value != newValue)
                stringList[index] = newValue;
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawVector2(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object temp = null;
            Dictionary<string, object> vectDict = null;
            Vector2 currentValue = Vector2.zero;
            Vector2 newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            if (data.TryGetValue(key, out temp))
            {
                vectDict = temp as Dictionary<string, object>;
                currentValue.x = Convert.ToSingle(vectDict["x"]);
                currentValue.y = Convert.ToSingle(vectDict["y"]);
            }
            
            newValue = EditorGUILayout.Vector2Field(label, currentValue);
            if (newValue != currentValue)
            {
                vectDict["x"] = newValue.x;
                vectDict["y"] = newValue.y;
                data[key] = vectDict;
            }
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    
    protected virtual void DrawListVector2(int index, Dictionary<string, object> value, List<object> vectorList)
    {
        try
        {
            Vector2 newValue;
            Vector2 currentValue = Vector2.zero;

            currentValue.x = Convert.ToSingle(value["x"]);
            currentValue.y = Convert.ToSingle(value["y"]);
            
            newValue = EditorGUILayout.Vector2Field(string.Format("{0}:", index), currentValue);
            if (newValue != currentValue)
            {
                value["x"] = newValue.x;
                value["y"] = newValue.y;
                vectorList[index] = value;
            }
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawVector3(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object temp = null;
            Dictionary<string, object> vectDict = null;
            Vector3 currentValue = Vector3.zero;
            Vector3 newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            if (data.TryGetValue(key, out temp))
            {
                vectDict = temp as Dictionary<string, object>;
                currentValue.x = Convert.ToSingle(vectDict["x"]);
                currentValue.y = Convert.ToSingle(vectDict["y"]);
                currentValue.z = Convert.ToSingle(vectDict["z"]);
            }
            
            newValue = EditorGUILayout.Vector3Field(label, currentValue);
            if (newValue != currentValue)
            {
                vectDict["x"] = newValue.x;
                vectDict["y"] = newValue.y;
                vectDict["z"] = newValue.z;
                data[key] = vectDict;
            }
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    
    protected virtual void DrawListVector3(int index, Dictionary<string, object> value, List<object> vectorList)
    {
        try
        {
            Vector3 newValue;
            Vector3 currentValue = Vector3.zero;
            
            currentValue.x = Convert.ToSingle(value["x"]);
            currentValue.y = Convert.ToSingle(value["y"]);
            currentValue.z = Convert.ToSingle(value["z"]);
            
            newValue = EditorGUILayout.Vector3Field(string.Format("{0}:", index), currentValue);
            if (newValue != currentValue)
            {
                value["x"] = newValue.x;
                value["y"] = newValue.y;
                value["z"] = newValue.z;
                vectorList[index] = value;
            }
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected virtual void DrawVector4(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object temp = null;
            Dictionary<string, object> vectDict = null;
            Vector4 currentValue = Vector4.zero;
            Vector4 newValue;
            string key = string.Format(EZConstants.MetaDataFormat, EZConstants.ValuePrefix, fieldName);
            
            if (data.TryGetValue(key, out temp))
            {
                vectDict = temp as Dictionary<string, object>;
                currentValue.x = Convert.ToSingle(vectDict["x"]);
                currentValue.y = Convert.ToSingle(vectDict["y"]);
                currentValue.z = Convert.ToSingle(vectDict["z"]);
                currentValue.w = Convert.ToSingle(vectDict["w"]);
            }
            
            newValue = EditorGUILayout.Vector4Field(label, currentValue);
            if (newValue != currentValue)
            {
                vectDict["x"] = newValue.x;
                vectDict["y"] = newValue.y;
                vectDict["z"] = newValue.z;
                data[key] = vectDict;
            }
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    
    protected virtual void DrawListVector4(int index, Dictionary<string, object> value, List<object> vectorList)
    {
        try
        {
            Vector4 newValue;
            Vector4 currentValue = Vector4.zero;
            
            currentValue.x = Convert.ToSingle(value["x"]);
            currentValue.y = Convert.ToSingle(value["y"]);
            currentValue.z = Convert.ToSingle(value["z"]);
            currentValue.w = Convert.ToSingle(value["w"]);
            
            newValue = EditorGUILayout.Vector4Field(string.Format("{0}:", index), currentValue);
            if (newValue != currentValue)
            {
                value["x"] = newValue.x;
                value["y"] = newValue.y;
                value["z"] = newValue.z;
                value["w"] = newValue.w;
                vectorList[index] = value;
            }
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

                EditorGUILayout.LabelField("Value:", GUILayout.Width(80));
                newIndex = EditorGUILayout.Popup(currentIndex, possibleValues.ToArray());

                if (newIndex != currentIndex)            
                    data[key] = possibleValues[newIndex];
            }
            else
            {
                EditorGUILayout.LabelField("Default Value: null", GUILayout.Width(110));
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

                EditorGUILayout.LabelField(string.Format("{0}:", index), GUILayout.Width(20));
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

    #region Filter/Sorting Methods
    protected virtual void DrawFilterSection()
    {
        // Text search
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Search By Key:", GUILayout.Width(80));
        filterText = EditorGUILayout.TextField(filterText);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region List Helper Methods
    protected virtual void ResizeList(List<object> list, int size, object defaultValue)
    {
        // Remove from the end until the size matches what we want
        if (list.Count > size)
            list.RemoveRange(size, list.Count-size);
        else if (list.Count < size)
        {
            // Add entries with the default value until the size is what we want
            for (int i = list.Count; i < size; i++) 
            {
                if (defaultValue != null && defaultValue.GetType().Equals(typeof(Dictionary<string, object>)))
                    list.Add(new Dictionary<string, object>((defaultValue as Dictionary<string, object>)));
                else
                    list.Add(defaultValue);
            }
        }
    }

    protected virtual object GetDefaultValueForType(BasicFieldType type)
    {
        object defaultValue = 0;
        if (type.IsSet(BasicFieldType.Vector2))
        {
            defaultValue = new Dictionary<string, object>() 
            {
                {"x", 0f},
                {"y", 0f}
            };
        }
        else if (type.IsSet(BasicFieldType.Vector3))
        {
            defaultValue = new Dictionary<string, object>() 
            {
                {"x", 0f},
                {"y", 0f},
                {"z", 0f}
            };
        }
        else if (type.IsSet(BasicFieldType.Vector4))
        {
            defaultValue = new Dictionary<string, object>() 
            {
                {"x", 0f},
                {"y", 0f},
                {"z", 0f},
                {"w", 0f}
            };
        }
        else if (type.IsSet(BasicFieldType.String))
        {
            defaultValue = "";
        }
        else
            defaultValue = 0;

        return defaultValue;
    }
    #endregion
    
    #region Abstract methods
    protected abstract void Load();
    protected abstract void Save();
    protected abstract void Create(object data);
    protected abstract void DrawEntry(string key, Dictionary<string, object> data);
    #endregion
}
