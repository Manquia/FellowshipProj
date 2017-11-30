using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    
    public Vector2 lookSensitivity = new Vector2(10.0f,10.0f);
    
    public float maxPitchUpAngle = 80.0f;
    public float maxPitchDownAngle = 70.0f;

    private Transform CameraTrans;

	// Use this for initialization
	void Start ()
    {
        CameraTrans = transform.Find("Camera");

        Debug.Assert(CameraTrans != null, "Camera is missing from camera");
        SetCursorState(CursorLockMode.Locked);
    }
    void Destroy()
    {
        SetCursorState(CursorLockMode.None);
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateRotation();
    }
    // Store this becuase Unity's rotation isn't 
    private float cameraTurn = 0.0f;
    private float cameraPitch = 0.0f;
    private void UpdateRotation()
    {
        float lookVecX = lookSensitivity.x * Input.GetAxis("Mouse X");
        float lookVecY = lookSensitivity.y * Input.GetAxis("Mouse Y");

        { // rotate player relative to mouse movement
            // Limit rotation
            var turnAngle = cameraTurn + lookVecX;

            var turnRot = Quaternion.AngleAxis(turnAngle, Vector3.up);
            transform.localRotation = turnRot;

            cameraTurn = turnAngle;
        }

        { // Pitch Camera
            // Limit rotation
            var pitchAngle = Mathf.Clamp(cameraPitch + lookVecY, -maxPitchDownAngle, maxPitchUpAngle);

            var pitchRot = Quaternion.AngleAxis(pitchAngle, -Vector3.right);
            CameraTrans.localRotation = pitchRot;

            cameraPitch = pitchAngle;
        }
    }


    void SetCursorState(CursorLockMode mode)
    {
        Cursor.lockState = mode;
        // Hide cursor when locking
        Cursor.visible = (CursorLockMode.Locked != mode);
    }
}
