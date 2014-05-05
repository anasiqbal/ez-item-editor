using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class GUITextSettings : MonoBehaviour {

    public string DataKey = "";

    //
    // Initialize the GUIText from the values in game data manager
    // specified by the DataKey.
    //
    public void Init () {
        
        // Dict to hold the Game Data 
        Dictionary<string, object> guiData;

        // Get the Game Data object 
        if (GDEDataManager.Instance.Get(DataKey, out guiData))
        {
            GUIText guiText = gameObject.GetComponent<GUIText>();

            // Pull out the text value from Game Data 
            string text;
            guiData.TryGetString("text", out text);

            if (text.Contains("{0}"))
                guiText.text = string.Format(text, GDEDataManager.Instance.DataFilePath);
            else
                guiText.text = text;

            // Pull out the size value from Game Data 
            int fontSize;
            guiData.TryGetInt("size", out fontSize);
            guiText.fontSize = fontSize;

            // Pull out the color value from Game Data
            Color textColor;
            guiData.TryGetColor("color", out textColor);
            guiText.color = textColor;

            // Pull out the position value from Game Data
            Vector3 position;
            guiData.TryGetVector3("position", out position);
            transform.localPosition = position;
        }
    }
}
