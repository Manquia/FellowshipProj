using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollowOnTrigger : FFComponent {
    FFAction.ActionSequence seq;

    public float distAlongPath = 0;
    [Range(0.1f, 100.0f)]
    public float movementSpeed;
    public int currentPointNumber = 0;
    public FFPath PathToFollow;

    // Use this for initialization
    void Start()
    {
        // Valid path
        if (PathToFollow != null && PathToFollow.points.Length > 1)
        {
            seq = action.Sequence();
            transform.position = PathToFollow.PointAlongPath(PathToFollow.LengthAlongPathToPoint(currentPointNumber));
            WaitForInput();
        }
        else
        {
            if (PathToFollow != null)
                Debug.Log("PathFollowOnTrigger has a path which has " + PathToFollow.points.Length + " points which is invalid.");
            else
                Debug.Log("PathFollowOnTrigger has no path to follow");
        }
    }

    // Wait For Input State
    void WaitForInput()
    {
        FFMessageBoard<TriggerObject>.Connect(OnTriggerObject, gameObject);
    }
    private void OnTriggerObject(TriggerObject e)
    {
        ++currentPointNumber;
        MoveForward();
    }

    // Move forward state
    void MoveForward()
    {
        FFMessageBoard<TriggerObject>.Disconnect(OnTriggerObject, gameObject);

        float lengthToNextPoint = PathToFollow.LengthAlongPathToPoint(currentPointNumber);
        if (distAlongPath >= lengthToNextPoint) // reached next point
        {
            transform.position = PathToFollow.PointAlongPath(lengthToNextPoint); // goto point
            distAlongPath = lengthToNextPoint;

            //Debug.Log("LengthToNextPoint (finished): " + lengthToNextPoint);

            WaitForInput(); // waitForInput
            return;
        }
        else // keep moving along path
        {
            transform.position = PathToFollow.PointAlongPath(distAlongPath);
        }

        distAlongPath += Time.deltaTime * movementSpeed;

        seq.Sync();
        seq.Call(MoveForward);
    }
    
    
}
