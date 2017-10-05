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
    public Transform trans;
}
struct StayCommand
{
    public Vector3 point;
}

public class PlayerController : FFComponent
{

    FFAction.ActionSequence transformationSeq;

    public bool active = false;
    
    public float m_movementSpeed = 3.2f;
    public float m_jumpSpeed = 3.5f;
    public float m_sprintMultiplier = 0.5f;

    public float m_maxMoveSpeed = 5.0f;
    public float m_movementDecay = 0.25f; // how quickly we reach our speed limits

    public float m_mouseSensitivity = 0.4f;


    public Color pigColor = Color.red;
    public Color GhostColor = Color.cyan;
    public float TransformationTime = 1.25f;

    public enum State
    {
        None,
        Ghost,
        Pig,
    }

    public Camera playerCamera;
    public State state = State.None;

    //private Rigidbody rigid; 

    // Use this for initialization
    void Start ()
    {
        transformationSeq = action.Sequence();
        //rigid = GetComponent<Rigidbody>();
        UpdatePlayerControllerData();
        
        FFMessage<ActivatePlayer>.Connect(OnActivatePlayer);
        FFMessage<DeactivatePlayer>.Connect(OnDeactivatePlayer);

        {// Setup colors for materials
            var pigMaterial = FFResource.Load_Material("Pig");
            var ghostMaterial = FFResource.Load_Material("Ghost");
            pigMaterial.color = pigColor;
            ghostMaterial.color = GhostColor;
        }
        

        // Start as a ghost
        ChangeState(State.Ghost);
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

    #region helpers

    void DisbleGO(object go)
    {
        ((GameObject)go).SetActive(false);
    }
    void EnableGo(object go)
    {
        ((GameObject)go).SetActive(true);
    }
    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }
    
    #endregion
    
    
    // Update is called once per frame
    void Update ()
    {
        {// Player Input
            var mousePos = Input.mousePosition;
            var mousePress = Input.GetMouseButtonDown(0);
            var ray = playerCamera.ScreenPointToRay(mousePos);
            RaycastHit rayHit;
            string[] layerNames = { "Default" };
            int raycastMask = LayerMask.GetMask(layerNames); // default, TODO Add floor
            var raycastHitSomething = Physics.Raycast(ray, out rayHit, 100.0f, raycastMask);
            

            // Interact/Move
            if (mousePress && raycastHitSomething)
            {
                var steering = GetComponent<Steering>();
                var targetPoint = rayHit.point + new Vector3(0, 0.5f, 0.0f); // offset 0.5 up
                steering.SetupTarget(rayHit.transform, targetPoint);
            }

            // Toggle Manifestation
            if(Input.GetKeyDown(KeyCode.T))
            {
                if (state == State.Pig)
                {
                    ChangeState(State.Ghost);
                }
                else if(state == State.Ghost)
                {
                    ChangeState(State.Pig);
                }
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
                    fc.trans = transform;
                    fc.point = GetPlayerPosition();
                    FFMessage<FollowCommand>.SendToLocal(fc);
                }
            }
        }

        
        {// Handle Movement

        }
	}



    private void ChangeState(State newState)
    {
        if (newState == state)
            return;
        
        // Get model roots
        GameObject pigObj = transform.Find("PigModel").gameObject;
        GameObject ghostObj = transform.Find("GhostModel").gameObject;

        // reset sequence
        transformationSeq.ClearSequence();

        //  get change model material
        var pigMaterial = FFResource.Load_Material("Pig");
        var ghostMaterial = FFResource.Load_Material("Ghost");
        var pigColorRef = new FFRef<Color>(() => pigMaterial.color, (v) => { pigMaterial.color = v; });
        var ghostColorRef = new FFRef<Color>(() => ghostMaterial.color, (v) => { ghostMaterial.color = v; });

        if (newState == State.Ghost) // turned into a ghost
        {
            // enable ghost model
            ghostObj.SetActive(true);

            // disable gravity
            var rigid = GetComponent<Rigidbody>();
            rigid.useGravity = false;

            // Can move through walls
            string ghostMask = "Ghost";
            gameObject.layer = LayerMask.NameToLayer(ghostMask);


            // Color property for each model mesh to change its color.
            transformationSeq.Property(pigColorRef, pigColor.MakeClear(), FFEase.E_SmoothStartEnd, TransformationTime);
            transformationSeq.Property(ghostColorRef, GhostColor, FFEase.E_SmoothStartEnd, TransformationTime);
            transformationSeq.Sync();
            transformationSeq.Call(DisbleGO, pigObj);
        }
        else if(newState == State.Pig) // turned into a pig
        {
            // enable pig model
            pigObj.SetActive(true);
            
            // enable gravity
            var rigid = GetComponent<Rigidbody>();
            rigid.useGravity = true;

            // Cannot move thorugh walls
            string physicalMask = "Physical";
            gameObject.layer = LayerMask.NameToLayer(physicalMask);
            
            // Color property for each model mesh to change its color.
            transformationSeq.Property(pigColorRef, pigColor, FFEase.E_SmoothStartEnd, TransformationTime);
            transformationSeq.Property(ghostColorRef, GhostColor.MakeClear(), FFEase.E_SmoothStartEnd, TransformationTime);
            transformationSeq.Sync();
            transformationSeq.Call(DisbleGO, ghostObj);
        }



        state = newState;
    }

    private void UpdateUI()
    {
        // Update UI related stuff
        if(Input.GetKey(KeyCode.Escape))
        {
            // @TODO Add event to setup the menu
        }
    }

    //Vector2 m_MouseMovement; // movement of the mouse between frames
    private void UpdatePlayerControllerData()
    {
        // calculate movement movement and update Mouse position
        {
            //m_MouseMovement = new Vector2(
            //    Input.GetAxis("Mouse X"),
            //    Input.GetAxis("Mouse Y"));
        }
    }
}
