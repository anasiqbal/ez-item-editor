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
    public const float MinTextAreaWidth = 100f;
    public const float MinTextAreaHeight = LineHeight;
    public const double DoubleClickTime = 0.5;
    #endregion

    #region Preference Keys
    public const string CreateDataColorKey = "ez_createdatacolor";
    public const string DefineDataColorKey = "ez_definedatacolor";
    public const string HighlightColorKey = "ez_highlightcolor";

    public const string CreateDataFileKey = "ez_createdatafile";
    public const string DefineDataFileKey = "ez_definedatafile";
    #endregion

    #region Default Preference Settings
    public const string CreateDataColor = "#013859";
    public const string DefineDataColor = "#185e65";
    public const string HighlightColor = "#f15c25";

    public const string CreateDataFile = "ezitems.json";
    public const string DefineDataFile = "ezschema.json";
    #endregion
}
