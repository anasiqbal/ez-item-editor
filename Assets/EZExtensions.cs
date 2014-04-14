using System;

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
}

