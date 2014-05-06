using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class GUITextSettings : MonoBehaviour {

    public Dictionary<string, object> Data;

    //
    // Initialize the GUIText from the values in the Data dictionary
    //
    public void Init () {
        
        if (Data != null)
        {
            GUIText guiText = gameObject.GetComponent<GUIText>();

            // Pull out the text value from Game Data 
            string text;
            Data.TryGetString("text", out text);

            if (text.Contains("{0}"))
                guiText.text = string.Format(text, Application.dataPath + "/GameDataEditor/Scenes/SquareExample/Resources/" + GDEDataManager.Instance.DataFilePath + ".json");
            else
                guiText.text = text;

            // Pull out the size value from Game Data 
            int fontSize;
            Data.TryGetInt("size", out fontSize);
            guiText.fontSize = fontSize;

            // Pull out the color value from Game Data
            Color textColor;
            Data.TryGetColor("color", out textColor);
            guiText.color = textColor;

            // Pull out the position value from Game Data
            Vector3 position;
            Data.TryGetVector3("position", out position);
            transform.localPosition = position;
        }
    }
}
