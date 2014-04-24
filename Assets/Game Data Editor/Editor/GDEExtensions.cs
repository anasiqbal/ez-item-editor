using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

namespace GameDataEditor.GDEExtensionMethods
{
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

