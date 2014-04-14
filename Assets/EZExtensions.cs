using System;
using System.Collections.Generic;

namespace EZExtensionMethods
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
}

