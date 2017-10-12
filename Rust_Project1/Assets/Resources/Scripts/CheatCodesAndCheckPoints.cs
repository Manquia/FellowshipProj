using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatCodesAndCheckPoints : MonoBehaviour {


    public Transform sierra;
    public Transform player;
    public Transform mainCamera;

    FFPath checkPointPath;

	// Use this for initialization
	void Start ()
    {
        checkPointPath = GetComponent<FFPath>();
    }
	
	// Update is called once per frame
	void Update ()
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
        }

        // Load Splash Screen
        if (Input.GetKeyDown(KeyCode.Alpha9)) SceneManager.LoadScene("SplashScreen");
        if (Input.GetKeyDown(KeyCode.Alpha0)) SceneManager.LoadScene("World");
    }


    void JumpToCheckPoint(int index)
    {
        Debug.Assert(index < checkPointPath.points.Length);

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
