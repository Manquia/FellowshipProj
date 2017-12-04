using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceSprite : MonoBehaviour {

    Transform playerCamera;
	// Use this for initialization
	void Start ()
    {
        playerCamera = GameObject.Find("Camera").transform;
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        var vecToPlayer = playerCamera.position - transform.position;
        var flatVecToPlayer = Vector3.ProjectOnPlane(vecToPlayer, Vector3.up);

        transform.LookAt(transform.position + flatVecToPlayer, Vector3.up);
	}
}
