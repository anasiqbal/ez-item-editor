using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class LoadTextSettings : MonoBehaviour {

    //
    // Initialize Game Data 
    //
    // Game Data can be initialized in the Start method. 
    // Start is called on the frame when the script is enabled.
    // 
    void Start () {
        
        // Dict to hold the Game Data 
        Dictionary<string, object> guiData;

        // Init the Game Data file
        GDEDataManager.Instance.Init(Application.dataPath + "/GameDataEditor/Scenes/celeste_test_data.json");

        // Get the Game Data object 
        if (GDEDataManager.Instance.Get("headerText", out guiData))
        {
            GUIText guiText = gameObject.GetComponent<GUIText>();

            // Pull out Game Data 
            string text;
            guiData.TryGetString("text", out text);
            guiText.text = text;

            // Pull out Game Data 
            int fontSize;
            guiData.TryGetInt("size", out fontSize);
            guiText.fontSize = fontSize;
        }
    }
}
