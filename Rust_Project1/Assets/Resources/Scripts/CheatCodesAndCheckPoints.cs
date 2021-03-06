﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

struct SetCheckpoint
{
    public int index;
}

struct ResetPlayerToLastCheckpoint
{
}


public class CheatCodesAndCheckPoints : MonoBehaviour {


    public Transform sierra;
    public Transform player;
    public Transform mainCamera;

    FFPath checkPointPath;

    [HideInInspector]
    public int currentCheckpoint = 0;

	// Use this for initialization
	void Start ()
    {
        currentCheckpoint = 0;
        checkPointPath = GetComponent<FFPath>();

        FFMessage<SetCheckpoint>.Connect(OnSetCheckpoint);
        FFMessage<ResetPlayerToLastCheckpoint>.Connect(OnResetPlayerToLastCheckpoint);

        SetupCheckPointCollisionPoints();
    }

    void OnDestroy()
    {
        FFMessage<SetCheckpoint>.Disconnect(OnSetCheckpoint);
        FFMessage<ResetPlayerToLastCheckpoint>.Disconnect(OnResetPlayerToLastCheckpoint);
    }

    void SetupCheckPointCollisionPoints()
    {
        var collisionSpherePrefab = FFResource.Load_Prefab("CheckPointCollisionSphere");

        for(int i = 0; i < checkPointPath.points.Length; ++i)
        {
            var position = checkPointPath.points[i];
            
            var newCheckPointCollisionSphere = Instantiate<GameObject>(collisionSpherePrefab);
            var checkPoint = newCheckPointCollisionSphere.GetComponent<CheckPointSphere>();

            newCheckPointCollisionSphere.transform.SetParent(transform, true);
            newCheckPointCollisionSphere.transform.position = position;
            checkPoint.index = i;
        }
    }


    private void OnResetPlayerToLastCheckpoint(ResetPlayerToLastCheckpoint e)
    {
        // @TODO. Show visuals that you have died/reset back to checkpoint?
        JumpToCheckPoint(currentCheckpoint);
    }

    private void OnSetCheckpoint(SetCheckpoint e)
    {
        // @TODO. Show visuals that checkpoint was reached maybe?
        currentCheckpoint = e.index;
    }
    
	
	// Update is called once per frame
	void Update ()
    {
        // Cheat Codes be here!
        {
            if(checkPointPath != null)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) JumpToCheckPoint(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) JumpToCheckPoint(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) JumpToCheckPoint(2);
                if (Input.GetKeyDown(KeyCode.Alpha4)) JumpToCheckPoint(3);
                if (Input.GetKeyDown(KeyCode.Alpha5)) JumpToCheckPoint(4);
                if (Input.GetKeyDown(KeyCode.Alpha6)) JumpToCheckPoint(5);
                if (Input.GetKeyDown(KeyCode.Alpha7)) JumpToCheckPoint(6);
                if (Input.GetKeyDown(KeyCode.Alpha8)) JumpToCheckPoint(7);
            }

            // Load Splash Screen
            //if (Input.GetKeyDown(KeyCode.Alpha9)) SceneManager.LoadScene("SplashScreen");
            //if (Input.GetKeyDown(KeyCode.Alpha0)) SceneManager.LoadScene("World");
        }
    }


    // Sets the Current checkpoint to this value
    void JumpToCheckPoint(int index)
    {
        currentCheckpoint = index;
        index = index % checkPointPath.points.Length;

        var characterPlacement = checkPointPath.transform.TransformPoint(checkPointPath.points[index]);
        var cameraPlacement = characterPlacement + (Vector3.up * 5.0f);

        {// move player
            var playerPlacement = characterPlacement + (Vector3.right * 0.7f);
            player.position = playerPlacement;
            player.GetComponent<Steering>().SetupTarget(null, playerPlacement);
        }

        {// move sierra
            var sierraPlacement = characterPlacement + (-Vector3.right * 0.7f);
            sierra.position = sierraPlacement;
            sierra.GetComponent<Steering>().SetupTarget(null, sierraPlacement);
        }

        {// move camera
            mainCamera.position = cameraPlacement;
        }
    }


    void ResetToCheckpoint(int index)
    {
        // Do more stuff?
        JumpToCheckPoint(index);

    }
}
