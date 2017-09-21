using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FloorCube : MonoBehaviour {

    /*
    // Update is called once per frame
    public int updateCounter = Random.Range(0,1000);
    void Update ()
    {
        ++updateCounter;
        if(updateCounter > 10)
        {
            updateCounter = 0;
            var scale = transform.localScale.x;
            var pos = transform.localPosition;


            var posXOffset = pos.x - Mathf.Floor(pos.x);
            var posYOffset = pos.y - Mathf.Floor(pos.y);
            var posZOffset = pos.z - Mathf.Floor(pos.z);
            
            // Half Scale
            if (scale > 0.45f && scale < 0.55f)
            {
                posXOffset -= 0.25f;
                posYOffset -= 0.25f;
                posZOffset -= 0.25f;

                if (posXOffset > 0.25f) posXOffset -= 0.5f;
                if (posYOffset > 0.25f) posYOffset -= 0.5f;
                if (posZOffset > 0.25f) posZOffset -= 0.5f;

                transform.localPosition = new Vector3(
                    pos.x - posXOffset,
                    pos.y - posYOffset,
                    pos.z - posZOffset);
            } 
            // Full Scale
            else if(scale > 0.95f && scale < 1.05f)
            {
                if (posXOffset > 0.51f) posXOffset -= 0.5f;
                if (posYOffset > 0.51f) posYOffset -= 0.5f;
                if (posZOffset > 0.51f) posZOffset -= 0.5f;

                transform.localPosition = new Vector3(
                    pos.x - posXOffset,
                    pos.y - posYOffset,
                    pos.z - posZOffset);
            }
        }
        
	}
    
		*/
}
