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

struct FollowCommand
{
    public Vector3 point;
}
struct StayCommand
{
    public Vector3 point;
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


    public Camera playerCamera;

    private Rigidbody rigid; 

    // Use this for initialization
    void Start ()
    {
        rigid = GetComponent<Rigidbody>();
        UpdatePlayerControllerData();
        
        FFMessage<ActivatePlayer>.Connect(OnActivatePlayer);
        FFMessage<DeactivatePlayer>.Connect(OnDeactivatePlayer);
	}
    void OnDestroy()
    {
        FFMessage<ActivatePlayer>.Disconnect(OnActivatePlayer);
        FFMessage<DeactivatePlayer>.Disconnect(OnDeactivatePlayer);
    }

    private void OnActivatePlayer(ActivatePlayer e)
    {
        active = true;
        UpdatePlayerControllerData();
    }

    private void OnDeactivatePlayer(DeactivatePlayer e)
    {
        UpdatePlayerControllerData();
        active = false;
    }


    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }


    // Update is called once per frame
    void Update ()
    {
        {// Player Input
            var mousePos = Input.mousePosition;
            var mousePress = Input.GetMouseButtonDown(0);
            var ray = playerCamera.ScreenPointToRay(mousePos);
            RaycastHit rayHit;
            var raycastHitSomething = Physics.Raycast(ray, out rayHit);
            

            // Interact/Move
            if (mousePress && raycastHitSomething)
            {
                GetComponent<Steering>().targetPoint = rayHit.point;
            }

            // Commands
            {
                // Stay
                if (Input.GetKeyDown(KeyCode.S))
                {
                    StayCommand sc;
                    sc.point = GetPlayerPosition();
                    FFMessage<StayCommand>.SendToLocal(sc);
                }

                // Follow
                if (Input.GetKeyDown(KeyCode.F))
                {
                    FollowCommand fc;
                    fc.point = GetPlayerPosition();
                    FFMessage<FollowCommand>.SendToLocal(fc);
                }
            }
        }

        
        {// Handle Movement

        }
	}
    

    private void UpdateUI()
    {
        // Update UI related stuff
        if(Input.GetKey(KeyCode.Escape))
        {
            // @TODO Add event to setup the menu
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
}
