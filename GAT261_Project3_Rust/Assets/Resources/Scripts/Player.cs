using System;
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

        // Offset Along Rope (vertical along rope)
        public float leftHandOffsetOnRope;
        public float rightHandOffsetOnRope;
        public float leftFootOffsetOnRope;
        public float rightFootOffsetOnRope;

        // Left/Right posiition OFfset
        public Vector3 leftHandOffset;
        public Vector3 rightHandOffset;
        public Vector3 leftFootOffset;
        public Vector3 rightFootOffset;

        // Rotations
        public Vector3 leftHandRot;
        public Vector3 rightHandRot;
        public Vector3 leftFootRot;
        public Vector3 rightFootRot;


        // Functional stuff
        public float angleOnRope; // in degrees
        public float distFromRope;
        public float rotationYaw;
        public float rotationPitch;
        
        // @TODO @Polish
        public float onRopeAngularVelocity; // <--- Rename...

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
    void OnDestroy()
    {
        if (OnRope != null)
            DestroyOnRope();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void FixedUpdate()
    {
    }

    private void OnRopeChange(RopeChange e)
    {
        UpdateRope(e.dt);
    }
    void UpdateRope(float dt)
    {
        var rope = OnRope.rope;
        var ropePath = rope.GetPath();
        var ropeLength = ropePath.PathLength;
        var ropeVecNorm = rope.RopeVecNorm();

        var distOnPath = Mathf.Clamp(ropeLength - OnRope.distUpRope, 0.0f, ropeLength);
        //var velocity = rope.VelocityAtLength(OnRope.distUpRope);

        // update Character Position
        var AngleFromDown = Quaternion.FromToRotation(Vector3.down, ropeVecNorm);
        var angularRotationOnRope = Quaternion.AngleAxis(OnRope.angleOnRope, ropeVecNorm) * AngleFromDown;
        var positionOnRope = ropePath.PointAlongPath(distOnPath);
        
        transform.position = positionOnRope +
            (angularRotationOnRope * -Vector3.forward * OnRope.distFromRope); // @ TODO: Add charater offset!
        var vecForward = positionOnRope - transform.position;
        

        //Debug.DrawLine(positionOnRope, transform.position, Color.yellow);
        var forwardRot = Quaternion.LookRotation(vecForward, -ropeVecNorm);
        transform.rotation = forwardRot;
        var characterRot = forwardRot * Quaternion.AngleAxis(OnRope.rotationPitch, transform.right) * Quaternion.AngleAxis(OnRope.rotationYaw, transform.forward);
        transform.rotation = characterRot;

        // update Snapping IK
        {
            ikSnap.rightHandPos = ropePath.PointAlongPath(distOnPath - OnRope.rightHandOffsetOnRope) + (angularRotationOnRope * OnRope.rightHandOffset);
            ikSnap.leftHandPos = ropePath.PointAlongPath(distOnPath - OnRope.leftHandOffsetOnRope) + (angularRotationOnRope * OnRope.leftHandOffset);
            ikSnap.rightFootPos = ropePath.PointAlongPath(distOnPath - OnRope.rightFootOffsetOnRope) + (angularRotationOnRope * OnRope.rightFootOffset);
            ikSnap.leftFootPos = ropePath.PointAlongPath(distOnPath - OnRope.leftFootOffsetOnRope) + (angularRotationOnRope * OnRope.leftFootOffset);
            
            ikSnap.rightHandRot = angularRotationOnRope * Quaternion.Euler(OnRope.rightHandRot);
            ikSnap.leftHandRot =  angularRotationOnRope * Quaternion.Euler(OnRope.leftHandRot) ;
            ikSnap.rightFootRot = angularRotationOnRope * Quaternion.Euler(OnRope.rightFootRot);
            ikSnap.leftFootRot =  angularRotationOnRope * Quaternion.Euler(OnRope.leftFootRot) ;
        }
    }


    void SetupOnRope()
    {
        SetVelocityRef(new FFRef<Vector3>(
            () => OnRope.rope.VelocityAtDistUpRope(OnRope.distUpRope),
            (v) => {} ));

        FFMessageBoard<RopeChange>.Connect(OnRopeChange, OnRope.rope.gameObject);
    }

    void DestroyOnRope()
    {
        if(OnRope.rope != null)
            FFMessageBoard<RopeChange>.Disconnect(OnRopeChange, OnRope.rope.gameObject);
    }
    
}
