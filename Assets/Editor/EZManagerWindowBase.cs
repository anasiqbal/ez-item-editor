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

    protected float currentLine = 0;
    protected float currentLinePosition = EZConstants.LeftBuffer;

    protected Dictionary<string, float> groupHeights = new Dictionary<string, float>();
    protected float scrollViewHeight = 0;
    protected float scrollViewY = 0;

    protected Dictionary<string, float> groupHeightBySchema = new Dictionary<string, float>();

    protected GUIStyle labelStyle = null;
    protected GUIStyle saveButtonStyle = null;
    protected string saveButtonText = "Save";
    protected bool needsSave = false;
     
    #region OnGUI and DrawHeader Methods
    protected virtual void OnGUI()
    {
        if (labelStyle == null)
            labelStyle = new GUIStyle(GUI.skin.label);

        if (saveButtonStyle == null)
            saveButtonStyle = new GUIStyle(GUI.skin.button);

        ResetToTop();
        DrawHeader();
    }

    protected virtual void DrawHeader()
    {
        float width = 40;
        if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Load"))
            Load();
        currentLinePosition += (width + 2);

        DrawDataFileLabelForHeader();

        NewLine();

        if (needsSave)
        {
            width = 90;
            saveButtonStyle.normal.textColor = Color.red;
            saveButtonStyle.fontStyle = FontStyle.Bold;
            saveButtonText = "Save Needed";
        }
        else
        {
            width = 40;
            saveButtonStyle.normal.textColor = GUI.skin.button.normal.textColor;
            saveButtonStyle.fontStyle = FontStyle.Normal;
            saveButtonText = "Save";
        }
        if (GUI.Button(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), saveButtonText, saveButtonStyle))
            Save();

        NewLine();
        
        DrawFilterSection();

        width = FullSeparatorWidth();
        GUI.Box(new Rect(currentLinePosition, TopOfLine(), width, 1), "");
    }
    #endregion

    #region GUI Position Methods
    protected virtual void ResetToTop()
    {
        currentLine = EZConstants.TopBuffer/EZConstants.LineHeight;
        currentLinePosition = EZConstants.LeftBuffer;
    }

    protected virtual void NewLine(float numNewLines = 1)
    {
        currentLine += numNewLines;
        currentLinePosition = EZConstants.LeftBuffer;
    }

    protected virtual float TopOfLine()
    {
        return EZConstants.LineHeight*currentLine;
    }

    protected virtual float MiddleOfLine()
    {
        return EZConstants.LineHeight*currentLine + EZConstants.LineHeight/2;
    }

    protected virtual float PopupTop()
    {
        return TopOfLine()+1;
    }

    protected virtual float StandardHeight()
    {
        return EZConstants.LineHeight-2;
    }

    protected virtual float TextBoxHeight()
    {
        return EZConstants.LineHeight-4;
    }

    protected virtual float VectorFieldHeight()
    {
        return EZConstants.LineHeight*1.2f;
    }

    protected virtual float FullSeparatorWidth()
    {
        return this.position.width-EZConstants.LeftBuffer-EZConstants.RightBuffer;
    }

    protected virtual float ScrollViewWidth()
    {
        return FullWindowWidth() - 20;
    }

    protected virtual float FullWindowWidth()
    {
        return this.position.width;
    }

    protected virtual float HeightToBottomOfWindow()
    {
        return this.position.height - (currentLine*EZConstants.LineHeight);
    }

    protected virtual float CurrentHeight()
    {
        return currentLine*EZConstants.LineHeight;
    }

    protected virtual bool IsVisible(float groupHeight)
    {
        float topSkip = this.verticalScrollbarPosition.y;            
        float bottomThreshold = CurrentHeight() + groupHeight;            
        if (topSkip >= bottomThreshold) {                
            // the group is above our current window                
            //Debug.Log(string.Format("Group is above topSkip:{0} bottomLine:{1}", topSkip,  bottomThreshold));
            return false;                
        }
        
        float bottomSkip = topSkip + scrollViewHeight + scrollViewY;            
        float topThreshold = CurrentHeight() - EZConstants.LineHeight;
        if (topThreshold >= bottomSkip) {                
            // the group is below our current window
            //Debug.Log(string.Format("Group is below bottomSkip:{0} topLine:{1}", bottomSkip,  topThreshold));
            return false;
        }
        
        return true;
    }

    protected virtual void SetGroupHeight(string forKey, float height)
    {
        if (groupHeights.ContainsKey(forKey))
            groupHeights[forKey] = height;
        else
            groupHeights.Add(forKey, height);
    }
    #endregion

    #region Foldout Methods
    protected virtual bool DrawFoldout(string label, string key)
    {
        bool currentFoldoutState = entryFoldoutState.Contains(key);

        float width = 200;
        bool newFoldoutState = EditorGUI.Foldout(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), currentFoldoutState, label, true);
        SetFoldout(newFoldoutState, key);

        NewLine();
            
        return newFoldoutState;
    }

    protected virtual void DrawExpandCollapseAllFoldout(string[] forKeys)
    {
        string label;
        if (currentFoldoutAllState)
            label = "Collapse All";
        else
            label = "Expand All";

        float width = 80;
        bool newFoldAllState = EditorGUI.Foldout(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), currentFoldoutAllState, label, true);
        if (newFoldAllState != currentFoldoutAllState) 
        {
            SetAllFoldouts(newFoldAllState, forKeys);
            currentFoldoutAllState = newFoldAllState;
            
            // Reset scrollbar if we just collapsed everything
            if (!newFoldAllState)
                verticalScrollbarPosition.y = 0;
        }

        NewLine();
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
        {
            entryFoldoutState.Remove(forKey);

            // Reset the group height to be a single line for the root foldout
            SetGroupHeight(forKey, EZConstants.LineHeight);
        }
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

            GUIContent content = new GUIContent(label);
            float width = labelStyle.CalcSize(content).x;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), content);
            currentLinePosition += (width + 2);

            width = 50;
            newValue = EditorGUI.Toggle(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), Convert.ToBoolean(currentValue));
            currentLinePosition += (width + 2);

            if (newValue != Convert.ToBoolean(currentValue))
            {
                data[key] = newValue;
                needsSave = true;
            }
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

            float width = 20;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), string.Format("{0}:", index));
            currentLinePosition += (width + 2);

            width = 30;
            newValue = EditorGUI.Toggle(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), value);
            currentLinePosition += (width + 2);

            if (value != newValue)
            {
                boolList[index] = newValue;
                needsSave = true;
            }
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

            GUIContent content = new GUIContent(label);
            float width = labelStyle.CalcSize(content).x;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), content);
            currentLinePosition += (width + 2);

            width = 50;
            newValue = EditorGUI.IntField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), Convert.ToInt32(currentValue));
            currentLinePosition += (width + 2);

            if (newValue != Convert.ToInt32(currentValue))
            {
                data[key] = newValue;
                needsSave = true;
            }
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

            float width = 20;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), string.Format("{0}:", index));
            currentLinePosition += (width + 2);

            width = 50;
            newValue = EditorGUI.IntField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), value);
            currentLinePosition += (width + 2);

            if (value != newValue)
            {
                intList[index] = newValue;
                needsSave = true;
            }
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

            GUIContent content = new GUIContent(label);
            float width = labelStyle.CalcSize(content).x;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), content);
            currentLinePosition += (width + 2);

            width = 50;
            newValue = EditorGUI.FloatField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), Convert.ToSingle(currentValue));
            currentLinePosition += (width + 2);

            if (newValue != Convert.ToSingle(currentValue))
            {
                data[key] = newValue;
                needsSave = true;
            }
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

            float width = 20;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), string.Format("{0}:", index));
            currentLinePosition += (width + 2);

            width = 50;
            newValue = EditorGUI.FloatField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), value);
            currentLinePosition += (width + 2);

            if (value != newValue)
            {
                floatList[index] = newValue;
                needsSave = true;
            }
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

            GUIContent content = new GUIContent(label);
            float width = labelStyle.CalcSize(content).x;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), content);
            currentLinePosition += (width + 2);

            width = 100;
            newValue = EditorGUI.TextField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), currentValue as string);
            currentLinePosition += (width + 2);

            if (newValue != (string)currentValue)
            {
                data[key] = newValue;
                needsSave = true;
            }
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

            float width = 20;
            EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), string.Format("{0}:", index));
            currentLinePosition += (width + 2);

            width = 100;
            newValue = EditorGUI.TextField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), value);
            currentLinePosition += (width + 2);

            if (value != newValue)
            {
                stringList[index] = newValue;
                needsSave = true;
            }
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

            float width = 136;
            newValue = EditorGUI.Vector2Field(new Rect(currentLinePosition, TopOfLine(), width, VectorFieldHeight()), label, currentValue);
            currentLinePosition += (width + 2);

            if (newValue != currentValue)
            {
                vectDict["x"] = newValue.x;
                vectDict["y"] = newValue.y;
                data[key] = vectDict;
                needsSave = true;
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

            float width = 136;
            newValue = EditorGUI.Vector2Field(new Rect(currentLinePosition, TopOfLine(), width, VectorFieldHeight()), string.Format("{0}:", index), currentValue);
            currentLinePosition += (width + 2);

            if (newValue != currentValue)
            {
                value["x"] = newValue.x;
                value["y"] = newValue.y;
                vectorList[index] = value;
                needsSave = true;
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

            float width = 200;
            newValue = EditorGUI.Vector3Field(new Rect(currentLinePosition, TopOfLine(), width, VectorFieldHeight()), label, currentValue);
            currentLinePosition += (width + 2);

            if (newValue != currentValue)
            {
                vectDict["x"] = newValue.x;
                vectDict["y"] = newValue.y;
                vectDict["z"] = newValue.z;
                data[key] = vectDict;
                needsSave = true;
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

            float width = 200;
            newValue = EditorGUI.Vector3Field(new Rect(currentLinePosition, TopOfLine(), width, VectorFieldHeight()), string.Format("{0}:", index), currentValue);
            currentLinePosition += (width + 2);

            if (newValue != currentValue)
            {
                value["x"] = newValue.x;
                value["y"] = newValue.y;
                value["z"] = newValue.z;
                vectorList[index] = value;
                needsSave = true;
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

            float width = 228;
            newValue = EditorGUI.Vector4Field(new Rect(currentLinePosition, TopOfLine(), width, VectorFieldHeight()), label, currentValue);
            currentLinePosition += (width + 2);

            if (newValue != currentValue)
            {
                vectDict["x"] = newValue.x;
                vectDict["y"] = newValue.y;
                vectDict["z"] = newValue.z;
                vectDict["w"] = newValue.w;
                data[key] = vectDict;
                needsSave = true;
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

            float width = 228;
            newValue = EditorGUI.Vector4Field(new Rect(currentLinePosition, TopOfLine(), width, VectorFieldHeight()), string.Format("{0}:", index), currentValue);
            currentLinePosition += (width + 2);

            if (newValue != currentValue)
            {
                value["x"] = newValue.x;
                value["y"] = newValue.y;
                value["z"] = newValue.z;
                value["w"] = newValue.w;
                vectorList[index] = value;
                needsSave = true;
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

            float width;
            if (canEdit && possibleValues != null)
            {
                currentIndex = possibleValues.IndexOf(currentValue as string);

                GUIContent content = new GUIContent("Value:");
                width = labelStyle.CalcSize(content).x;
                EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), content);
                currentLinePosition += (width + 2);

                width = 80;
                newIndex = EditorGUI.Popup(new Rect(currentLinePosition, PopupTop(), width, StandardHeight()), currentIndex, possibleValues.ToArray());
                currentLinePosition += (width + 2);

                if (newIndex != currentIndex)   
                {
                    data[key] = possibleValues[newIndex];                    
                    needsSave = true;
                }
            }
            else
            {
                GUIContent content = new GUIContent("Default Value: null");
                width = labelStyle.CalcSize(content).x;
                EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), content);
                currentLinePosition += (width + 4);
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
            float width;

            if (canEdit && possibleValues != null)
            {
                currentIndex = possibleValues.IndexOf(value);

                width = 20;
                EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), string.Format("{0}:", index));
                currentLinePosition += (width + 2);

                width = 80;
                newIndex = EditorGUI.Popup(new Rect(currentLinePosition, PopupTop(), width, StandardHeight()), currentIndex, possibleValues.ToArray());
                currentLinePosition += (width + 2);

                if (newIndex != currentIndex)     
                {
                    customList[index] = possibleValues[newIndex];
                    needsSave = true;
                }
            }
            else
            {
                GUIContent content = new GUIContent(string.Format("{0}: null", index));
                width = labelStyle.CalcSize(content).x;
                EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), content);
                currentLinePosition += (width + 2);
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
        float width = 45;
        EditorGUI.LabelField(new Rect(currentLinePosition, TopOfLine(), width, StandardHeight()), "Search:");

        currentLinePosition += (width + 8);

        width = 180;
        filterText = EditorGUI.TextField(new Rect(currentLinePosition, TopOfLine(), width, TextBoxHeight()), filterText);
        currentLinePosition += (width + 2);
    }

    protected virtual int NumberOfItemsBeingShown(Dictionary<string, Dictionary<string, object>> data)
    {
        int resultCount = 0;
        
        foreach(KeyValuePair<string, Dictionary<string, object>> pair in data)
        {
            if (!ShouldFilter(pair.Key, pair.Value))
                resultCount++;
        }
        
        return resultCount;
    }
    #endregion

    #region List Helper Methods
    protected virtual void ResizeList(List<object> list, int size, object defaultValue)
    {
        // Remove from the end until the size matches what we want
        if (list.Count > size)
        {
            list.RemoveRange(size, list.Count-size);
            needsSave = true;
        }
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
            needsSave = true;
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
    protected abstract void Remove(string key);

    protected abstract void DrawEntry(string key, Dictionary<string, object> data);
    protected abstract void DrawDataFileLabelForHeader();

    protected abstract bool ShouldFilter(string key, Dictionary<string, object> data);

    protected abstract float CalculateGroupHeightsTotal();
    #endregion
}
