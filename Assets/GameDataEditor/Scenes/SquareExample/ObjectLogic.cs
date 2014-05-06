using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class ObjectLogic : MonoBehaviour {

    public Dictionary<string, object> Data;

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
        if (Data != null)
        {
            // Pull out the position value from Game Data
            Vector3 startPosition;
            Data.TryGetVector3("position", out startPosition);
            transform.localPosition = startPosition;
            
            // Pull out the minScale value from Game Data
            Data.TryGetVector3("minScale", out minScale);
            transform.localScale = minScale;
            
            // Pull out the maxScale value from Game Data
            Data.TryGetVector3("maxScale", out maxScale);
            targetScale = maxScale;
            
            // Pull out the scaleSpeed value from Game Data
            Data.TryGetFloat("scaleSpeed", out scaleSpeed);
            
            // Pull out the colorSpeed value from Game Data
            Data.TryGetFloat("colorSpeed", out colorSpeed);
            
            
            // Pull out the colors list from Game Data
            object temp;
            Data.TryGetValue("colors", out temp);
            
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

            if (colors != null && colors.Count > 0)
                renderer.material.color = colors[0];
        }
    }	

    // Update is called once per frame
    void Update () {

        // If we have reached the maxScale, set the target Scale to minScale
        // to start shrinking the gameobject. If we have reached the minScale, set the target
        // to the maxScale to start expanding the gameobject.
        if (transform.localScale.NearlyEqual(maxScale))        
            targetScale = minScale;
        else if (transform.localScale.NearlyEqual(minScale))        
            targetScale = maxScale;

        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);


        // Rotate through the colors
        if (colors != null && colors.Count > 0 && colorSpeed > 0)
        {
            if (renderer.material.color.NearlyEqual(colors[currentColorIndex]))        
                currentColorIndex = GetNextColorIndex();

            renderer.material.color = Color.Lerp(renderer.material.color, colors[currentColorIndex], colorSpeed * Time.deltaTime);
        }
    }

    int GetNextColorIndex()
    {
        int nextIndex = currentColorIndex+1;

        if (nextIndex == colors.Count)
            nextIndex = 0;

        return nextIndex;
    }
}
