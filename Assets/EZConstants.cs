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
    public const string TemplateKey = "_ezTemplate";
    #endregion

    #region Window Constants
    public const int IndentSize = 20;
    #endregion
}
