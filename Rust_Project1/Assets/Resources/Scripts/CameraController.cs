using UnityEngine;
using System.Collections;
using System;

public enum CameraState
{
    Player,     // Height always enabled
    Transition, // Height sometimes enabled
    Cinimatic,  // Height always disabled
}
struct CameraControlEvent
{
    public CameraState state;
}

struct CameraTransitionEvent
{
    // Stop all current transitions and do this one!
    public bool flushTransitions;
    // Raycast and check, only if orientation doesn't change
    public bool updateHeightDistance;
    
    public Vector3 position;
    public Quaternion rotation;
    public int fieldOfView;

    public float timeToComplete;
    public FFEase ease;
}
struct CameraCinimaticEvent
{
    Animation cameraAnim;
}

public class CameraController : FFComponent {


    FFAction.ActionSequence GroundRaySequence;
    FFAction.ActionSequence HeightSequence;
    FFAction.ActionSequence TransitionSequence;

    CameraState state = CameraState.Player;
    Camera cameraComp;
    Transform cameraTrans;


	// Use this for initialization
	void Start ()
    {
        { // Events
            FFMessage<CameraControlEvent>.Connect(OnCameraControl);
            FFMessage<CameraTransitionEvent>.Connect(OnCameraTransition);
        }

        { // Initialization
            GroundRaySequence = action.Sequence();
            HeightSequence = action.Sequence();
            TransitionSequence = action.Sequence();

            cameraTrans = transform;
            cameraComp = cameraTrans.GetComponent<Camera>();
        }


        { // Start Update Sequences
            GroundRaySequence.Call(UpdateGroundRay);
            HeightSequence.Call(UpdateCameraHeight);
        }
    }
    void OnDestroy()
    {
        
        { // Events
            FFMessage<CameraControlEvent>.Disconnect(OnCameraControl);
            FFMessage<CameraTransitionEvent>.Disconnect(OnCameraTransition);
        }
    }

    #region Events

    private void OnCameraControl(CameraControlEvent e)
    {
        state = e.state;
    }


    FFAction.ActionSequence transitionSequence;
    private void OnCameraTransition(CameraTransitionEvent e)
    {
        if (e.flushTransitions)
        {
            TransitionSequence.ClearSequence();
        }

        // position
        if (cameraTrans.position != e.position)
            TransitionSequence.Property(
                cameraTrans.ffposition(),
                e.position,
                e.ease,
                e.timeToComplete);

        // @ TODO, rotation
        //if (cameraTrans.rotation != e.rotation)
        //    TransitionSequence.Property(
        //        cameraTrans.ffrotation(),
        //        e.rotation,
        //        e.ease,
        //        e.timeToComplete);

        // field of view
        if (cameraComp.fieldOfView != e.fieldOfView)
            TransitionSequence.Property(
                cameraComp.fffieldofview(),
                e.fieldOfView,
                e.ease,
                e.timeToComplete);



        TransitionSequence.Sync();
    }
    #endregion

    #region Sequence Updaters
    public float cameraHeight = 10.0f; // based on camera look dist
    FFVar<float> deltaHeight = new FFVar<float>(0.0f);
    void UpdateGroundRay()
    {
        { // GroundPhysics Raycast
            Vector3 ori = cameraTrans.position;
            Vector3 dir = cameraTrans.TransformVector(Vector3.forward);
            RaycastHit hit;

            // Ground Raycast
            if (Physics.Raycast(ori,dir,out hit, 10.0f))
            {
                // Hit is distance is different
                if(Mathf.Abs((hit.distance - deltaHeight) - cameraHeight) > 0.25f)
                {
                    deltaHeight.Setter(hit.distance - cameraHeight);
                    HeightSequence.ClearSequence();
                    UpdateCameraHeight();
                }
            }
        }

        { // Do raycast again in a little while
            GroundRaySequence.Delay(0.08f);
            GroundRaySequence.Sync();
            GroundRaySequence.Call(UpdateGroundRay);
        }
    }
    
    void UpdateCameraHeight()
    {
        // Adjust height
        if(deltaHeight != 0.0f)
        {
            HeightSequence.Property(
                cameraTrans.ffposition(),
                cameraTrans.ffposition() + cameraTrans.TransformVector(Vector3.forward) * deltaHeight,
                FFEase.E_SmoothStartEnd,
                0.6f);
            
            // Change value in the same way we do the the positions so that raycast
            // can ignore the camera movement
            HeightSequence.Property(
                deltaHeight,
                0,
                FFEase.E_SmoothStartEnd,
                0.6f);
        }
    }
    #endregion

    // Update is called once per frame
    void FixedUpdate ()
    {
        // Camera is controlled by player
        if (state == CameraState.Player)
        {
            UpdatePan();
            UpdateZoom();
        }
	}


    
    void UpdatePan()
    {
        // Get the direction we want to move
        Vector3 moveVector = MoveVector();

        // have some movement
        if(moveVector != Vector3.zero)
        {
            var vecForward = cameraTrans.TransformVector(Vector3.forward);
            var vecRight2D = Vector3.Cross(vecForward, Vector3.down);
            var vecUp2D = Vector3.Cross(Vector3.down, vecRight2D);

            Debug.DrawLine(cameraTrans.position, cameraTrans.position + vecRight2D, Color.red); // Debug
            Debug.DrawLine(cameraTrans.position, cameraTrans.position + vecUp2D, Color.green);  // Debug

            // Apply horizontal movement
            cameraTrans.position = new Vector3(
                cameraTrans.position.x + (moveVector.x * vecRight2D.x),
                cameraTrans.position.y,
                cameraTrans.position.z + (moveVector.x * vecRight2D.z));

            // Apply Vertical movement
            cameraTrans.position = new Vector3(
                cameraTrans.position.x + (moveVector.z * vecUp2D.x),
                cameraTrans.position.y,
                cameraTrans.position.z + (moveVector.z * vecUp2D.z));
        }
    }
    Vector3 MoveVector()
    {
        Vector3 moveVector = Vector3.zero;
        { // Get move vector

            // Move up
            if (Input.GetKey(KeyCode.UpArrow))
            {
                moveVector += new Vector3(0, 0, 1);
            }

            // Move down
            if(Input.GetKey(KeyCode.DownArrow))
            {
                moveVector += new Vector3(0, 0, -1);
            }

            // Move left
            if(Input.GetKey(KeyCode.LeftArrow))
            {
                moveVector += new Vector3(-1, 0, 0);
            }

            // Move right
            if(Input.GetKey(KeyCode.RightArrow))
            {
                moveVector += new Vector3(1, 0, 0);
            }
        }
        return moveVector;
    }

    public float ZoomSpeed = 1.5f;
    public Vector2 ZoomLimits = new Vector2(2, 12);
    void UpdateZoom()
    {
        Vector2 zoomVector = ZoomVector();
        float zoomAmount = -zoomVector.y;
        
        cameraHeight = Mathf.Clamp(cameraHeight + (zoomAmount * ZoomSpeed), ZoomLimits.x, ZoomLimits.y);
    }
    Vector2 ZoomVector()
    {
        return Input.mouseScrollDelta;
    }
}
