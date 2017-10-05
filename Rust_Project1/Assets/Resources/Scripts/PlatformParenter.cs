using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformParenter : MonoBehaviour {

    void Start ()
    {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Character>() != null)
        {
            other.transform.SetParent(transform, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.GetComponent<Character>() != null)
        {
            other.transform.SetParent(null, true);
        }
    }
}
