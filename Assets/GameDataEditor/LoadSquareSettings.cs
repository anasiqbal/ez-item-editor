﻿using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class LoadSquareSettings : MonoBehaviour {

    Vector3 minScale;
    Vector3 maxScale;
    Vector3 targetScale;

    float scaleSpeed;

    //
    // Initialize Game Data 
    //
    // Game Data can be initialized in the Start method. 
    // Start is called on the frame when the script is enabled.
    // 
	void Start () {
        Dictionary<string, object> squareData;
        GDEDataManager.Instance.Init(Application.dataPath + "/GameDataEditor/Scenes/celeste_test_data.json");
        
        if (GDEDataManager.Instance.Get("square", out squareData))
        {
            Vector3 startPosition;
            squareData.TryGetVector3("position", out startPosition);
            gameObject.transform.localPosition = startPosition;

            squareData.TryGetVector3("minScale", out minScale);
            gameObject.transform.localScale = minScale;

            squareData.TryGetVector3("maxScale", out maxScale);
            targetScale = maxScale;

            squareData.TryGetFloat("scaleSpeed", out scaleSpeed);
        }
	}
	
	// Update is called once per frame
	void Update () {

        // If we have reached the maxScale, set the target Scale to minScale
        // to start shrinking the square. If we have reached the minScale, set the target
        // to the maxScale to start expanding the square.
        if (transform.localScale.x.NearlyEqual(maxScale.x) &&
            transform.localScale.y.NearlyEqual(maxScale.y) &&
            transform.localScale.z.NearlyEqual(maxScale.z))
        {
            targetScale = minScale;
        }
        else if (transform.localScale.x.NearlyEqual(minScale.x) &&
                 transform.localScale.y.NearlyEqual(minScale.y) &&
                 transform.localScale.z.NearlyEqual(minScale.z))
        {
            targetScale = maxScale;
        }

        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
	}
}
