using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;

public class InitScene : MonoBehaviour {

    public string DataFilePath;

    //
    // Initialize Game Data 
    //
    // Game Data can be initialized in the Start method. 
    // Start is called on the frame when the script is enabled.
    // 
    // Here we will instantiate all our objects in the scene and populate them
    // with our game data.
    //
	void Start () {        
        if (GDEDataManager.Instance.Init(Application.dataPath + DataFilePath))
        {
            List<string> dataKeys;

            // Init all of our GUITexts
            GDEDataManager.Instance.GetAllDataKeysBySchema("GuiText", out dataKeys);
            foreach(string dataKey in dataKeys)
            {
                GameObject guiText = new GameObject(dataKey);
                guiText.AddComponent<GUIText>();

                GUITextSettings textSettings = guiText.AddComponent<GUITextSettings>();
                textSettings.DataKey = dataKey;
                textSettings.Init();
            }

            // Init all of our Objects
            GDEDataManager.Instance.GetAllDataKeysBySchema("Object", out dataKeys);
            foreach(string dataKey in dataKeys)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ObjectLogic logic = cube.AddComponent<ObjectLogic>();
                logic.DataKey = dataKey;
                logic.Init();
            }
        }
        else
            Debug.Log("GDEManager not initialized!");
	}
}
