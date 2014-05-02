using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class ObjectLogic : MonoBehaviour {

    public string DataKey;

    Vector3 minScale;
    Vector3 maxScale;
    Vector3 targetScale;
    float scaleSpeed;

    List<Color> colors;
    int currentColorIndex;
    float colorSpeed;

    //
    // Initialize the Game Object from the values in game data manager
    // specified by the DataKey.
    //
    public void Init()
    {    
        Dictionary<string, object> squareData;
        
        if (GDEDataManager.Instance.Get(DataKey, out squareData))
        {
            Vector3 startPosition;
            squareData.TryGetVector3("position", out startPosition);
            transform.localPosition = startPosition;
            
            squareData.TryGetVector3("minScale", out minScale);
            transform.localScale = minScale;
            
            squareData.TryGetVector3("maxScale", out maxScale);
            targetScale = maxScale;
            
            squareData.TryGetFloat("scaleSpeed", out scaleSpeed);
            
            squareData.TryGetFloat("colorSpeed", out colorSpeed);
            
            
            // Load the color list
            object temp;
            squareData.TryGetValue("colors", out temp);
            
            colors = new List<Color>();
            List<object> colorDicts = temp as List<object>;
            foreach(object colorData in colorDicts)
            {
                Dictionary<string, object> colorDictionary = colorData as Dictionary<string, object>;
                Color c = new Color();
                
                colorDictionary.TryGetFloat("r", out c.r);
                colorDictionary.TryGetFloat("g", out c.g);
                colorDictionary.TryGetFloat("b", out c.b);
                colorDictionary.TryGetFloat("a", out c.a);
                
                colors.Add(c);
            }
        }
    }	

	// Update is called once per frame
	void Update () {

        // If we have reached the maxScale, set the target Scale to minScale
        // to start shrinking the square. If we have reached the minScale, set the target
        // to the maxScale to start expanding the square.
        if (transform.localScale.NearlyEqual(maxScale))        
            targetScale = minScale;
        else if (transform.localScale.NearlyEqual(minScale))        
            targetScale = maxScale;

        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);


        // Rotate through the colors
        if (renderer.material.color.NearlyEqual(colors[currentColorIndex]))        
            currentColorIndex = GetNextColorIndex();

        renderer.material.color = Color.Lerp(renderer.material.color, colors[currentColorIndex], colorSpeed * Time.deltaTime);
	}

    int GetNextColorIndex()
    {
        int nextIndex = currentColorIndex+1;

        if (nextIndex == colors.Count)
            nextIndex = 0;

        return nextIndex;
    }
}
