//
//
// A simple First-person perspective player controller. 
//
//


using UnityEngine;
using System.Collections;
using System;

struct ActivatePlayer
{
}
struct DeactivatePlayer
{
}

public class PlayerController : MonoBehaviour
{
    public bool active = false;
    
    public float m_movementSpeed = 3.2f;
    public float m_jumpSpeed = 3.5f;
    public float m_sprintMultiplier = 0.5f;

    public float m_maxMoveSpeed = 5.0f;
    public float m_movementDecay = 0.25f; // how quickly we reach our speed limits

    public float m_mouseSensitivity = 0.4f;
    
    public void ToggleMouseCapture()
    {
        if (Cursor.visible == false)
            CaptureMouse(true);
        else
            CaptureMouse(false);
    }
    public void CaptureMouse(bool capture)
    {
        if(capture)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined | CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private Rigidbody rigid; 

    // Use this for initialization
    void Start ()
    {
        CaptureMouse(true);
        rigid = GetComponent<Rigidbody>();
        UpdatePlayerControllerData();
        
        FFMessage<ActivatePlayer>.Connect(OnActivatePlayer);
        FFMessage<DeactivatePlayer>.Connect(OnDeactivatePlayer);
	}

    private void OnActivatePlayer(ActivatePlayer e)
    {
        active = true;
        CaptureMouse(true);
        UpdatePlayerControllerData();
    }

    private void OnDeactivatePlayer(DeactivatePlayer e)
    {
        UpdatePlayerControllerData();
        CaptureMouse(false);
        active = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMouseCapture();
        }


    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (active == false) return;

        UpdatePlayerControllerData();
        UpdateDirectionalMovement();
        UpdatePlayerJump();

        // UpdateRotation(); // moved to camera controller

        UpdateUI();

	
	}
    

    private void UpdateUI()
    {
        // Update UI related stuff
        if(Input.GetKey(KeyCode.C))
        {
        }
    }

    Vector2 m_MouseMovement; // movement of the mouse between frames
    private void UpdatePlayerControllerData()
    {
        // calculate movement movement and update Mouse position
        {
            m_MouseMovement = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y"));
        }
    }
    private void UpdateDirectionalMovement()
    {
        var forwardKey = KeyCode.W;
        var backKey = KeyCode.S;
        var leftKey = KeyCode.A;
        var rightKey = KeyCode.D;

        var sprintKey = KeyCode.LeftShift;

        float movementSpeed = m_movementSpeed;
        float maxMovementSpeed = m_maxMoveSpeed;
        Vector3 movementDirection = new Vector3();

        // Directional Input
        if (Input.GetKey(forwardKey)) movementDirection += Vector3.forward;
        if (Input.GetKey(leftKey)) movementDirection += Vector3.left;
        if (Input.GetKey(rightKey)) movementDirection += Vector3.right;
        if (Input.GetKey(backKey)) movementDirection += Vector3.back;

        if (movementDirection == Vector3.zero) // no input
            maxMovementSpeed = 0.0f; // do not move

        // Sprint Input
        if (Input.GetKey(sprintKey))
        {
            maxMovementSpeed *= m_sprintMultiplier;
            movementSpeed *= m_sprintMultiplier;
        }


        // Apply velocity to player
        {
            rigid.AddForce((movementSpeed * Time.fixedDeltaTime) * (transform.rotation * movementDirection), ForceMode.Impulse);

            var playerVelocityXZ = new Vector2(
                rigid.velocity.x,
                rigid.velocity.z);

            var playerSpeed = playerVelocityXZ.magnitude;


            // Limit speed, rubber band down style
            if (playerSpeed > maxMovementSpeed)
            {
                var speedReduction = ((playerSpeed - maxMovementSpeed) * m_movementDecay);
                var newPlayerSpeed = playerSpeed - speedReduction;
                
                var playerVelocityXZNorm = playerVelocityXZ.normalized;
                var newPlayerVelocityXZ = playerVelocityXZNorm * newPlayerSpeed;

                rigid.velocity = new Vector3(
                    newPlayerVelocityXZ.x,
                    Mathf.Clamp(rigid.velocity.y, -3.0f, 1.5f),
                    newPlayerVelocityXZ.y);
            }
        }
    }

    private void UpdatePlayerJump()
    {
        bool canJump = false;

        // Able to Jump
        // Touching Ground
        {
            RaycastHit hit;
            var col = GetComponent<CapsuleCollider>();

            // 10% longer ray than character half of the collider height
            float dist = (transform.lossyScale.y * col.height + col.radius) * (0.5f * 1.1f);
            //Debug.DrawLine(transform.position, transform.position + (Vector3.down * dist));
            if (Physics.Raycast( transform.position, Vector3.down, out hit, dist))
            {
                //Debug.Log("Ray Hit: " + hit.collider.name);
                canJump = true;
            }
        }

        // Jumping
        if(canJump && Input.GetKeyDown(KeyCode.Space))
        {
            var rigid = GetComponent<Rigidbody>();

            rigid.AddForce(
                Vector3.up * m_jumpSpeed,
                ForceMode.VelocityChange);
        }
    }

    // Store this becuase Unity's rotation isn't stable
    private float cameraPitch = 0.0f;
    private void UpdateRotation()
    {
        { // rotate player relative to mouse movement
            transform.Rotate(0.0f, m_MouseMovement.x * m_mouseSensitivity, 0.0f);
        }

        { // Pitch Camera
            var playerCamera = transform.Find("Camera");
            var rotationAxis = new Vector3(1, 0, 0);
            var rotationDegrees = (-m_MouseMovement.y * m_mouseSensitivity * 0.8f);

            // Limit rotation
            rotationDegrees = Mathf.Clamp(cameraPitch + rotationDegrees, -90.0f, 90.0f) - cameraPitch;

            var pitchRot = Quaternion.AngleAxis(rotationDegrees, rotationAxis);
            playerCamera.rotation *= pitchRot;

            cameraPitch += rotationDegrees;
        }
    }
    private void UpdateCursorState()
    {
        var escapeKey = KeyCode.Escape;
        if(Input.GetKeyDown(escapeKey))
        {
            CaptureMouse(false);
        }
    }
}
