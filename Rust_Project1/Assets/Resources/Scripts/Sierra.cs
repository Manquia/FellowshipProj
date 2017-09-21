using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sierra : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        FFMessage<StayCommand>.Connect(OnStayCommand);
        FFMessage<FollowCommand>.Connect(OnFollowCommand);

    }
    void OnDestroy()
    {
        FFMessage<StayCommand>.Disconnect(OnStayCommand);
        FFMessage<FollowCommand>.Disconnect(OnFollowCommand);
    }

    private void OnFollowCommand(FollowCommand e)
    {
        var steering = GetComponent<Steering>();

        steering.targetPoint = e.point;
        steering.TargetTrans = e.trans;
    }

    private void OnStayCommand(StayCommand e)
    {
        var steering = GetComponent<Steering>();

        steering.targetPoint = e.point;
        steering.TargetTrans = null;
    }

}
