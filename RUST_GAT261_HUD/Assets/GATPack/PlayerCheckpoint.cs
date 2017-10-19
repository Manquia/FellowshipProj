using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckpoint : MonoBehaviour {

    public bool IsStartingCheckpoint = false;



    private static Transform lastCheckpoint = null;
    public static Transform GetLastCheckpointObj()
    {
        return lastCheckpoint;
    }
    public static void SetLastCheckpointObj(Transform newCheckpoint)
    {
        lastCheckpoint = newCheckpoint;
    }


	// Use this for initialization
	void Start ()
    {
        if (IsStartingCheckpoint)
        {
            Debug.Assert(GetLastCheckpointObj() == null, "Multiple Player Checkpoints which are set to StartinCheckpoint");
            SetLastCheckpointObj(transform);
        }
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            //Debug.Log("Player Entered Checkpoint");
            SetLastCheckpointObj(transform);
        }
    }
}
