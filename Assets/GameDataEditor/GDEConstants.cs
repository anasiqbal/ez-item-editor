using UnityEngine;
using System.Collections;

namespace GameDataEditor
{
    public class GDEConstants {
        #region Metadata Constants
        public const string MetaDataFormat = "{0}{1}"; //{0} is the metadata prefix, {1} is the field the metadata is for

        // Field metadata prefixes
        public const string TypePrefix = "_gdeType_";
        public const string IsListPrefix = "_gdeIsList_";
        #endregion

        #region Item Metadata Constants
        public const string SchemaKey = "_gdeSchema";
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
        public const string CreateDataColorKey = "gde_createdatacolor";
        public const string DefineDataColorKey = "gde_definedatacolor";
        public const string HighlightColorKey = "gde_highlightcolor";

        public const string CreateDataFileKey = "gde_createdatafile";
        public const string DefineDataFileKey = "gde_definedatafile";
        #endregion

        #region Default Preference Settings
        public const string CreateDataColor = "#013859";
        public const string DefineDataColor = "#185e65";
        public const string HighlightColor = "#f15c25";

        public const string CreateDataFile = "gde_data.json";
        public const string DefineDataFile = "gde_schema.json";
        #endregion
    }
}
