using UnityEngine;
using System.Collections;
using System;

public enum CameraState
{
    Player,     // Height always enabled
    Menu,
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

    public CameraState state = CameraState.Menu;

    Camera cameraComp;
    Transform cameraTrans;

    public Transform playerTrans;
    public Vector2 cameraPanOffset;

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
            string[] rayMask = { "Default", "CameraFloor" };

            // Ground Raycast
            if (Physics.Raycast(ori,dir,out hit, 10.0f, LayerMask.GetMask(rayMask)))
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



    public float panSpeed = 8.4f;

    void UpdatePan()
    {
        // Get the direction we want to move
        Vector3 moveVector = MoveVector();

        // have some movement
        if(moveVector != Vector3.zero)
        {
            var vecForward = cameraTrans.forward;
            var vecRight2D = cameraTrans.right;
            var vecUp2D = cameraTrans.up;

            //Debug.DrawLine(cameraTrans.position, cameraTrans.position + vecRight2D, Color.red); // Debug
            //Debug.DrawLine(cameraTrans.position, cameraTrans.position + vecUp2D, Color.green);  // Debug

            // Apply horizontal movement
            cameraTrans.position = new Vector3(
                cameraTrans.position.x + (moveVector.z * vecRight2D.x) * panSpeed * Time.fixedDeltaTime,
                cameraTrans.position.y,
                cameraTrans.position.z + (moveVector.z * vecRight2D.z) * panSpeed * Time.fixedDeltaTime);

            // Apply Vertical movement
            cameraTrans.position = new Vector3(
                cameraTrans.position.x + (moveVector.x * vecUp2D.x) * panSpeed * Time.fixedDeltaTime,
                cameraTrans.position.y,
                cameraTrans.position.z + (moveVector.x * vecUp2D.z) * panSpeed * Time.fixedDeltaTime);
        }
    }
    public Vector3 moveVec;

    Vector3 MoveVector()
    {
        Vector3 moveVector = Vector3.zero;
        { // Get move vector
            var cameraOffset = new Vector3(cameraPanOffset.y, 0.0f, cameraPanOffset.x);
            var vecToPlayer = playerTrans.position - (cameraTrans.position + cameraOffset);
            var vecToPlayerXZ = new Vector3(-vecToPlayer.x, 0.0f, vecToPlayer.z);
            moveVec = vecToPlayerXZ;
        }
        return moveVec;
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
