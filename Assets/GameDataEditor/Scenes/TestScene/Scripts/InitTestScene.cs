using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class InitTestScene : MonoBehaviour 
{
    public GUIText BoolField;
    public GUIText BoolListField;
    public GUIText IntField;
    public GUIText IntListField;
    public GUIText FloatField;
    public GUIText FloatListField;
    public GUIText StringField;
    public GUIText StringListField;
    public GUIText Vector2Field;
    public GUIText Vector2LisField;
    public GUIText Vector3Field;
    public GUIText Vector3ListField;
    public GUIText Vector4Field;
    public GUIText Vector4ListField;
    public GUIText ColorField;
    public GUIText ColorListField;
    public GUIText CustomField;
    public GUIText CustomListField;

    public GUIText GetAllDataBySchema;
    public GUIText GetAllKeysBySchema;

    public GUIText Status;

	void Start () 
    {
        if (GDEDataManager.Instance.Init("test_scene_data"))
        {
            try
            {
                Dictionary<string, object> testData;
                GDEDataManager.Instance.Get("test_data", out testData);

                // Bool
                BoolField.text = "Bool Field: ";
                bool boolFieldValue;
                testData.TryGetBool("bool_field", out boolFieldValue);
                BoolField.text += boolFieldValue;

                // Bool List
                BoolListField.text = "Bool List Field: ";
                List<bool> boolListFieldValue;
                testData.TryGetBoolList("bool_list_field", out boolListFieldValue);
                foreach(bool boolVal in boolListFieldValue)
                    BoolListField.text += string.Format("{0} ", boolVal);

                // Int
                IntField.text = "Int Field: ";
                int intFieldValue;
                testData.TryGetInt("int_field", out intFieldValue);
                IntField.text += intFieldValue;

                // Int List
                IntListField.text = "Int List Field: ";
                List<int> intListFieldValue;
                testData.TryGetIntList("int_list_field", out intListFieldValue);
                foreach(int intVal in intListFieldValue)
                    IntListField.text += string.Format("{0} ", intVal);

                // Float
                FloatField.text = "Float Field: ";
                float floatFieldValue;
                testData.TryGetFloat("float_field", out floatFieldValue);
                FloatField.text += floatFieldValue;

                // Float List
                FloatListField.text = "Float List Field: ";
                List<float> floatListFieldValue;
                testData.TryGetFloatList("float_list_field", out floatListFieldValue);
                foreach(float floatVal in floatListFieldValue)
                    FloatListField.text += string.Format("{0} ", floatVal);

                // String
                StringField.text = "String Field: ";
                string stringFieldValue;
                testData.TryGetString("string_field", out stringFieldValue);
                StringField.text += stringFieldValue;

                // String List
                StringListField.text = "String List Field: ";
                List<string> stringListFieldValue;
                testData.TryGetStringList("string_list_field", out stringListFieldValue);
                foreach(string stringVal in stringListFieldValue)
                    StringListField.text += string.Format("{0} ", stringVal);

                // Vector2
                Vector2Field.text = "Vector2 Field: ";
                Vector2 vector2FieldValue;
                testData.TryGetVector2("vector2_field", out vector2FieldValue);
                Vector2Field.text += string.Format("({0}, {1})", vector2FieldValue.x, vector2FieldValue.y);

                // Vector2 List
                Vector2LisField.text = "Vector2 List Field: ";
                List<Vector2> vector2ListFieldValue;
                testData.TryGetVector2List("vector2_list_field", out vector2ListFieldValue);
                foreach(Vector2 vec2Val in vector2ListFieldValue)
                    Vector2LisField.text += string.Format("({0}, {1}) ", vec2Val.x, vec2Val.y);

                // Vector3
                Vector3Field.text = "Vector3 Field: ";
                Vector3 vector3FieldValue;
                testData.TryGetVector3("vector3_field", out vector3FieldValue);
                Vector3Field.text += string.Format("({0}, {1}, {2})", vector3FieldValue.x, vector3FieldValue.y, vector3FieldValue.z);

                // Vector3 List
                Vector3ListField.text = "Vector3 List Field: ";
                List<Vector3> vector3ListFieldValue;
                testData.TryGetVector3List("vector3_list_field", out vector3ListFieldValue);
                foreach(Vector3 vec3Val in vector3ListFieldValue)
                    Vector3ListField.text += string.Format("({0}, {1}, {2}) ", vec3Val.x, vec3Val.y, vec3Val.z);

                // Vector4
                Vector4Field.text = "Vector4 Field: ";
                Vector4 vector4FieldValue;
                testData.TryGetVector4("vector4_field", out vector4FieldValue);
                Vector4Field.text += string.Format("({0}, {1}, {2}, {3})", vector4FieldValue.x, vector4FieldValue.y, vector4FieldValue.z, vector4FieldValue.w);

                // Vector4 List
                Vector4ListField.text = "Vector4 List Field: ";
                List<Vector4> vector4ListFieldValue;
                testData.TryGetVector4List("vector4_list_field", out vector4ListFieldValue);
                foreach(Vector4 vec4Val in vector4ListFieldValue)
                    Vector4ListField.text += string.Format("({0}, {1}, {2}, {3}) ", vec4Val.x, vec4Val.y, vec4Val.z, vec4Val.w); 

                // Color
                ColorField.text = "Color Field: ";
                Color colorFieldValue;
                testData.TryGetColor("color_field", out colorFieldValue);
                ColorField.text += colorFieldValue.ToString();

                // Color List
                ColorListField.text = "Color List Field: ";
                List<Color> colorListFieldValue;
                testData.TryGetColorList("color_list_field", out colorListFieldValue);
                foreach(Color colVal in colorListFieldValue)
                    ColorListField.text += string.Format("{0}   ", colVal);

                // Custom
                CustomField.text = "Custom Field: ";
                string customFieldValue;
                testData.TryGetString("custom_field", out customFieldValue);
                Dictionary<string, object> customFieldData;
                GDEDataManager.Instance.Get(customFieldValue, out customFieldData);
                string customDataFieldString;
                customFieldData.TryGetString("description", out customDataFieldString);
                CustomField.text += customDataFieldString;

                // Custom List
                CustomListField.text = "Custom List Field:" + Environment.NewLine;
                List<string> customListFieldValue;
                testData.TryGetStringList("custom_list_field", out customListFieldValue);
                foreach(string customKey in customListFieldValue)
                {
                    string description;
                    Dictionary<string, object> customData;
                    GDEDataManager.Instance.Get(customKey, out customData);
                    customData.TryGetString("description", out description);
                    CustomListField.text += string.Format("     {0}{1}", description, Environment.NewLine);
                }


                // Get All Data By Schema
                GetAllDataBySchema.text = "Get All Data By Schema:" + Environment.NewLine;
                Dictionary<string, object> allDataByCustomSchema;
                GDEDataManager.Instance.GetAllDataBySchema("Custom", out allDataByCustomSchema);
                foreach(KeyValuePair<string, object> pair in allDataByCustomSchema)
                {
                    string description;
                    Dictionary<string, object> customData = pair.Value as Dictionary<string, object>;
                    customData.TryGetString("description", out description);
                    GetAllDataBySchema.text += string.Format("     {0}{1}", description, Environment.NewLine);
                }

                // Get All Keys By Schema
                GetAllKeysBySchema.text = "Get All Keys By Schema: ";
                List<string> customKeys;
                GDEDataManager.Instance.GetAllDataKeysBySchema("Custom", out customKeys);
                foreach(string key in customKeys)
                    GetAllKeysBySchema.text += string.Format("{0} ", key);

                Status.text = "Status: Everything looks great!";
            }
            catch(Exception ex)
            {
                Status.text = "Status: Something went wrong. See console for exception text.";
                Debug.LogException(ex);
            }
        }
        else
        {
            Status.text = "Status: Something went wrong. GDEDataManager was not initialized!";
        }
	}
}
