using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {


    public DynamicAudioPlayer dynAudioPlayer;
    public IK_Snap ikSnap;

    private FFRef<Vector3> velocityRef;
    private FFRef<Vector3> GetVelocityRef()
    {
        return velocityRef;
    }
    void SetVelocityRef(FFRef<Vector3> velocityRef)
    {
        Debug.Assert(velocityRef != null);
        this.velocityRef = velocityRef;

    }

    [System.Serializable]
    public class RopeConnection
    {
        public RopeController rope;
        public float distUpRope;

        public float leftHandOffset;
        public float rightHandOffset;
        public float leftFootOffset;
        public float rightFootOffset;

        public Vector3 playerControllerOffset;
        public Quaternion playerControllerRot;
    }
    public RopeConnection OnRope;

	// Use this for initialization
	void Start ()
    {
        // Set referece to 0 for initialization
        SetVelocityRef(new FFVar<Vector3>(Vector3.zero));
        dynAudioPlayer.SetDynamicValue(
            new FFRef<float>(
                () => GetVelocityRef().Getter().magnitude,
                (v) => {})
                );

        if (OnRope != null)
            SetupOnRope();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void FixedUpdate()
    {
        // On rope we have different controls
        if (OnRope != null)
        {
            UpdateRope(Time.fixedDeltaTime);
        }
    }

    void UpdateRope(float dt)
    {
        var rope = OnRope.rope;
        var ropePath = rope.GetPath();
        var ropeLength = ropePath.PathLength;

        var distOnPath = Mathf.Clamp(ropeLength - OnRope.distUpRope, 0.0f, ropeLength);
        //var velocity = rope.VelocityAtLength(OnRope.distUpRope);

        // update Character Position
        var ropeRot = OnRope.rope.RopeRotation();     // <----------------- THIS SHOULD BE BASED off of a angle at which we 
                                                      // should be facing the rope that way we can just change that value when
                                                      // we want to modify relative rotation to the rope for polish stuff.
        
        transform.position = ropePath.PointAlongPath(distOnPath) + (ropeRot * OnRope.playerControllerOffset);
        // Update Charcter Rotation // @ NEEDS WORK
        transform.rotation = ropeRot;

        // update Snapping IK
        {
            ikSnap.rightHandPos = ropePath.PointAlongPath(distOnPath - OnRope.rightHandOffset);
            ikSnap.leftHandPos = ropePath.PointAlongPath(distOnPath - OnRope.leftHandOffset);
            ikSnap.rightFootPos = ropePath.PointAlongPath(distOnPath - OnRope.rightFootOffset);
            ikSnap.leftFootPos = ropePath.PointAlongPath(distOnPath - OnRope.leftFootOffset);
            
            // @TODO, rotation
        }
    }


    void SetupOnRope()
    {
        SetVelocityRef(new FFRef<Vector3>(
            () => OnRope.rope.VelocityAtDistUpRope(OnRope.distUpRope),
            (v) => {} ));
    }
}
