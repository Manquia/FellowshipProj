using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillBetweenSelfAndTarget : MonoBehaviour {

    public Transform target;

    Transform physicalSelf;

	// Use this for initialization
	void Start ()
    {
        physicalSelf = transform.GetChild(0);
    }
	
	// Update is called once per frame
	void Update ()
    {
        { // FillBetweenSelfAndTarget
            var vecToTarget = target.position - transform.position;
            var midpoint = transform.position + (vecToTarget * 0.5f);



            // Be between target and self
            physicalSelf.position = midpoint;

            // Face target
            physicalSelf.rotation = Quaternion.LookRotation(Vector3.Normalize(vecToTarget), Vector3.up);


            //physicalSelf.localScale = 

        }
		
	}
}
