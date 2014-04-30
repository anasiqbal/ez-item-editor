using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class LoadTextSettings : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Dictionary<string, object> guiData;
        GDEDataManager.Instance.Init(Application.dataPath + "/GameDataEditor/Scenes/celeste_test_data.json");

        if (GDEDataManager.Instance.Get("headerText", out guiData))
        {
            GUIText guiText = gameObject.GetComponent<GUIText>();

            string text;
            guiData.TryGetString("text", out text);
            guiText.text = text;

            int fontSize;
            guiData.TryGetInt("size", out fontSize);
            guiText.fontSize = fontSize;
        }
	}
}
