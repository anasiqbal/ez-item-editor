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
}

