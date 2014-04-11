using UnityEngine;
using System.Collections;

public class EZConstants {
    #region Metadata Constants
    public const string MetaDataFormat = "{0}_{1}"; //{0} is the metadata prefix, {1} is the field the metadata is for

    // Field metadata prefixes
    public const string ValuePrefix = "_ezValue";
    public const string IsListPrefix = "_ezIsList";
    #endregion

    #region Item Metadata Constants
    public const string SchemaKey = "_ezSchema";
    #endregion

    #region Window Constants
    public const int Indent = 20;
    public const float LineHeight = 20f;
    public const float TopBuffer = 2f;
    public const float LeftBuffer = 2f;
    public const float RightBuffer = 2f;
    public const float VectorFieldBuffer = 0.75f;
    #endregion
}
