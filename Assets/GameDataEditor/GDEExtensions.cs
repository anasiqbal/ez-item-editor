using UnityEngine;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameDataEditor.GDEExtensionMethods
{
    public static class GenericExtensions
    {
        public static bool IsCloneableType<T>(this T variable)
        {
            return typeof(ICloneable).IsAssignableFrom(variable.GetType());
        }
        
        public static bool IsGenericList<T>(this T variable)
        {
            foreach (Type @interface in variable.GetType().GetInterfaces()) {
                if (@interface.IsGenericType) {
                    if (@interface.GetGenericTypeDefinition() == typeof(IList<>)) {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public static bool IsGenericDictionary<T>(this T variable)
        {
            foreach (Type @interface in variable.GetType().GetInterfaces()) {
                if (@interface.IsGenericType) {
                    if (@interface.GetGenericTypeDefinition() == typeof(IDictionary<,>)) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    
    public static class FlagExtensions
    {
        public static bool IsSet(this Enum variable, Enum flag)
        {
            ulong variableVal = Convert.ToUInt64(variable);
            ulong flagVal = Convert.ToUInt64(flag);
            return (variableVal & flagVal) == flagVal;
        }
    }
    
    public static class FloatExtensions
    {
        public const float TOLERANCE = 0.0001f;
        public static bool NearlyEqual(this float a, float b)
        {
            return Math.Abs(a - b) < TOLERANCE;
        }
    }
    
    public static class ArrayExtensions
    {
        public static bool IsValidIndex(this Array variable, int index)
        {
            return index > -1 && index < variable.Length;
        }
    }
    
    public static class ListExtensions
    {
        public static MethodInfo DeepCopyMethodInfo = typeof(ListExtensions).GetMethod("DeepCopy");
        public static List<T> DeepCopy<T>(this List<T> variable)
        {
            List<T> newList = new List<T>();
            
            T newEntry = default(T);
            foreach (T entry in variable)
            {
                if (entry == null)
                {
                    newEntry = entry;
                }
                else if (entry.IsCloneableType())
                {
                    newEntry = (T)((ICloneable)(entry)).Clone();
                }
                else if (entry.IsGenericList())
                {
                    Type listType = entry.GetType().GetGenericArguments()[0];
                    MethodInfo deepCopyMethod = DeepCopyMethodInfo.MakeGenericMethod(new Type[] { listType });
                    newEntry = (T)deepCopyMethod.Invoke(entry, new object[] {entry});
                }
                else if (entry.IsGenericDictionary())
                {
                    Type[] genericArgs = entry.GetType().GetGenericArguments();
                    Type keyType = genericArgs[0];
                    Type valueType = genericArgs[1];
                    
                    MethodInfo deepCopyMethod = DictionaryExtensions.DeepCopyMethodInfo.MakeGenericMethod(new Type[] { keyType, valueType });
                    newEntry = (T)deepCopyMethod.Invoke(entry, new object[] {entry});
                }
                else
                {
                    newEntry = entry;
                }
                
                newList.Add(newEntry);
            }
            return newList;
        }
        
        public static List<int> AllIndexesOf<T>(this List<T> variable, T searchValue) 
        {
            List<int> indexes = new List<int>();
            for (int index = 0; index<= variable.Count; index ++) 
            {
                index = variable.IndexOf(searchValue, index);
                if (index == -1)
                    break;
                
                indexes.Add(index);
            }
            return indexes;
        }
    }
    
    public static class DictionaryExtensions
    {
        // Adds the value if the key does not exist, otherwise it updates the value for the given key
        public static bool TryAddOrUpdateValue<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, TValue value)
        {
            bool result;
            try
            {
                if (variable.ContainsKey(key))
                {
                    variable[key] = value;
                    result = true;
                }
                else
                    result = variable.TryAddValue(key, value);
            }
            catch
            {
                result = false;
            }
            
            return result;
        }
        
        public static bool TryAddValue<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, TValue value)
        {
            bool result;
            try
            {
                variable.Add(key, value);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static bool TryGetBool<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out bool value)
        {
            bool result = true;
            value = false;

            try
            {
                TValue origValue;
                variable.TryGetValue(key, out origValue);
                value = Convert.ToBoolean(origValue);
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        public static bool TryGetString<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out string value)
        {
            bool result = true;
            value = "";
            
            try
            {
                TValue origValue;
                variable.TryGetValue(key, out origValue);
                value = origValue.ToString();
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        public static bool TryGetFloat<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out float value)
        {
            bool result = true;
            value = 0f;
            
            try
            {
                TValue origValue;
                variable.TryGetValue(key, out origValue);
                value = Convert.ToSingle(origValue);
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        public static bool TryGetInt<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out int value)
        {
            bool result = true;
            value = 0;
            
            try
            {
                TValue origValue;
                variable.TryGetValue(key, out origValue);
                value = Convert.ToInt32(origValue);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static bool TryGetVector2<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out Vector2 value)
        {
            bool result = true;
            value = Vector2.zero;
            
            try
            {
                TValue temp;
                Dictionary<string, object> vectorDict;
                variable.TryGetValue(key, out temp);
                
                vectorDict = temp as Dictionary<string, object>;
                if (vectorDict != null)
                {
                    value.x = Convert.ToSingle(vectorDict["x"]);
                    value.y = Convert.ToSingle(vectorDict["y"]);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        public static bool TryGetVector3<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out Vector3 value)
        {
            bool result = true;
            value = Vector3.zero;
            
            try
            {
                TValue temp;
                Dictionary<string, object> vectorDict;
                variable.TryGetValue(key, out temp);
                
                vectorDict = temp as Dictionary<string, object>;
                if (vectorDict != null)
                {
                    value.x = Convert.ToSingle(vectorDict["x"]);
                    value.y = Convert.ToSingle(vectorDict["y"]);
                    value.z = Convert.ToSingle(vectorDict["z"]);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static bool TryGetVector4<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out Vector4 value)
        {
            bool result = true;
            value = Vector4.zero;
            
            try
            {
                TValue temp;
                Dictionary<string, object> vectorDict;
                variable.TryGetValue(key, out temp);
                
                vectorDict = temp as Dictionary<string, object>;
                if (vectorDict != null)
                {
                    value.x = Convert.ToSingle(vectorDict["x"]);
                    value.y = Convert.ToSingle(vectorDict["y"]);
                    value.z = Convert.ToSingle(vectorDict["z"]);
                    value.w = Convert.ToSingle(vectorDict["w"]);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        public static MethodInfo DeepCopyMethodInfo = typeof(DictionaryExtensions).GetMethod("DeepCopy");
        public static Dictionary<TKey, TValue> DeepCopy<TKey, TValue>(this Dictionary<TKey, TValue> variable)
        {
            Dictionary<TKey, TValue> newDictionary = new Dictionary<TKey, TValue>();
            
            TKey newKey = default(TKey);
            TValue newValue = default(TValue);
            
            foreach (KeyValuePair<TKey, TValue> pair in variable)
            {
                if (pair.Key == null)
                    newKey = pair.Key;
                else if (pair.Key.IsCloneableType())
                {
                    newKey = (TKey)((ICloneable)(pair.Key)).Clone();
                }
                else if (pair.Key.IsGenericList())
                {
                    Type listType = pair.Key.GetType().GetGenericArguments()[0];                   
                    MethodInfo deepCopyMethod = ListExtensions.DeepCopyMethodInfo.MakeGenericMethod(new Type[] { listType });
                    newKey = (TKey)deepCopyMethod.Invoke(pair.Key, new object[] {pair.Key});
                }
                else if (pair.Key.IsGenericDictionary())
                {
                    Type[] genericArgs = pair.Key.GetType().GetGenericArguments();
                    Type keyType = genericArgs[0];
                    Type valueType = genericArgs[1];
                    
                    MethodInfo deepCopyMethod = DeepCopyMethodInfo.MakeGenericMethod(new Type[] { keyType, valueType });
                    newKey = (TKey)deepCopyMethod.Invoke(pair.Key, new object[] {pair.Key});
                }
                else
                    newKey = pair.Key;
                
                if (pair.Value == null)
                    newValue = pair.Value;
                else if (pair.Value.IsCloneableType())
                {
                    newValue = (TValue)((ICloneable)(pair.Value)).Clone();
                }
                else if (pair.Value.IsGenericList())
                {
                    Type listType = pair.Value.GetType().GetGenericArguments()[0];                   
                    MethodInfo deepCopyMethod = ListExtensions.DeepCopyMethodInfo.MakeGenericMethod(new Type[] { listType });
                    newValue = (TValue)deepCopyMethod.Invoke(pair.Value, new object[] {pair.Value});
                }
                else if (pair.Value.IsGenericDictionary())
                {
                    Type[] genericArgs = pair.Value.GetType().GetGenericArguments();
                    Type keyType = genericArgs[0];
                    Type valueType = genericArgs[1];
                    
                    MethodInfo deepCopyMethod = DeepCopyMethodInfo.MakeGenericMethod(new Type[] { keyType, valueType });
                    newValue = (TValue)deepCopyMethod.Invoke(pair.Value, new object[] {pair.Value});
                }
                else
                {
                    newValue = pair.Value;
                }
                
                newDictionary.Add(newKey, newValue);
            }
            return newDictionary;
        }
    }
    
    public static class StringExtensions
    {
        // Returns a new string that hightlights the first instance of substring with html color tag
        // Ex. "The sky is <color=blue>blue</color>!"
        public static string HighlightSubstring(this string variable, string substring, string color)
        {
            string highlightedString = "";
            
            if (!string.IsNullOrEmpty(substring))
            {
                int index = variable.Replace("Schema:", "       ").IndexOf(substring, StringComparison.CurrentCultureIgnoreCase);
                
                if (index != -1)
                    highlightedString = string.Format("{0}<color={1}>{2}</color>{3}", 
                                                      variable.Substring(0, index), color, variable.Substring(index, substring.Length), variable.Substring(index+substring.Length));
                else
                    highlightedString = variable.Clone() as string;
            }
            else
                highlightedString = variable.Clone() as string;
            
            return highlightedString;
        }
    }
    
    public static class ColorExtensions
    {
        public static string ToHexString(this Color32 color)
        {
            return string.Format("{0}{1}{2}", color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"));
        }
        
        public static Color ToColor(this string hex)
        {
            hex = hex.Replace("#", "");
            
            byte r = byte.Parse(hex.Substring(0,2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2,2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4,2), NumberStyles.HexNumber);
            
            return new Color32(r, g, b, 1);
        }
    }
}

