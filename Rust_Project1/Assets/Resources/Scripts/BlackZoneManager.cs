using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackZoneManager : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        // On Start turn on the blackZone parent so that they start working!!
        transform.GetChild(0).gameObject.SetActive(true);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
