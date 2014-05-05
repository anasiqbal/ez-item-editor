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
            Dictionary<string, object> objectData;

            // Get the square's data
            GDEDataManager.Instance.Get("square", out objectData);
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ObjectLogic cubeLogic = cube.AddComponent<ObjectLogic>();
            cubeLogic.Data = objectData;
            cubeLogic.Init();

            // Get the circle's data
            GDEDataManager.Instance.Get("circle", out objectData);
            GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ObjectLogic circleLogic = circle.AddComponent<ObjectLogic>();
            circleLogic.Data = objectData;
            circleLogic.Init();

            // Init all of our GUITexts
            List<Dictionary<string, object>> guiTextDataList;
            GDEDataManager.Instance.GetAllDataBySchema("GuiText", out guiTextDataList);
            foreach(Dictionary<string, object> data in guiTextDataList)
            {
                GameObject guiText = new GameObject("GUIText");
                guiText.AddComponent<GUIText>();

                GUITextSettings textSettings = guiText.AddComponent<GUITextSettings>();
                textSettings.Data = data;
                textSettings.Init();
            }
        }
        else
            Debug.Log("GDEManager not initialized!");
	}
}
