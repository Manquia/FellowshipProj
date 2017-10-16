using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathFollowOnTrigger))]
public class ResetPathFollowOnTriggerOnReset : MonoBehaviour {

    public CheatCodesAndCheckPoints checkpoints;

    // Use this for initialization
    void Start ()
    {
        FFMessage<ResetPlayerToLastCheckpoint>.Connect(OnResetPLayerToLastCheckpoint);
		
	}
    void OnDestroy()
    {
        FFMessage<ResetPlayerToLastCheckpoint>.Disconnect(OnResetPLayerToLastCheckpoint);
    }

    private void OnResetPLayerToLastCheckpoint(ResetPlayerToLastCheckpoint e)
    {
        if(checkpoints.currentCheckpoint == 6) // on 7th checkpoint only
        {
            var followPathOnTrigger = GetComponent<PathFollowOnTrigger>();
            followPathOnTrigger.currentPointNumber = 0;
            followPathOnTrigger.distAlongPath = 0.0f;
            followPathOnTrigger.MoveForward();
        }
    }
}
