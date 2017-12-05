using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public CameraController cameraController;
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

        public float pumpSpeed = 0.003f;
        public float pumpAcceleration = 0.5f;
        public float pumpResetSpeed = 0.05f;

        public float climbSpeed = 0.5f;
        public float rotateSpeed = 20.0f;
        public float leanSpeed = 0.9f;
        
        public float distUpRope;
        public float distPumpUp = 0.0f;
        
        public float maxPumpUpDist = 0.1f;
        public float maxPumpDownDist = 0.0f;

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
        bool modifier = Input.GetKey(KeyCode.LeftShift);
        float dt = Time.deltaTime;

        float pumpAmount = OnRope.pumpSpeed * dt;
        float climbAmount = OnRope.climbSpeed * dt;
        float rotateAmount = OnRope.rotateSpeed * dt;
        float leanAmount = OnRope.leanSpeed * dt;
        
        {
            bool up = Input.GetKey(KeyCode.W);
            bool down = Input.GetKey(KeyCode.S);
            bool left = Input.GetKey(KeyCode.A);
            bool right = Input.GetKey(KeyCode.D);
            bool space = Input.GetKey(KeyCode.Space);


            Vector3 leanVec = Vector3.zero;
            float climbVec = 0.0f;

            if (space)
            {
                RopePump(pumpAmount);
            }

            bool flipClimbMod = false;
            // going up
            if (up)
            {
                if (modifier == flipClimbMod)
                    leanVec += new Vector3(0.0f, 0.0f, 1.0f);
                else
                    climbVec += 1.0f;
            }
            // going down
            if (down)
            {
                if (modifier == flipClimbMod)
                    leanVec += new Vector3(0.0f, 0.0f, -1.0f);
                else
                    climbVec += -1.0f;
            }

            bool flipRotateMod = false;
            // going right
            if (right && !left)
            {
                if (modifier == flipRotateMod)
                    leanVec += new Vector3(1.0f, 0.0f, 0.0f);
            }
            // going left
            if (left && !right)
            {
                if (modifier == flipRotateMod)
                    leanVec += new Vector3(-1.0f, 0.0f, 0.0f);
            }

            // Pump
            if (space)
            {
                RopePump(pumpAmount);
            }
            else
            {
                var vecToRestingPump = Mathf.Clamp(
                    -OnRope.distPumpUp,
                    -OnRope.pumpResetSpeed * dt,
                     OnRope.pumpResetSpeed * dt);

                RopePump(vecToRestingPump);
            }

            //Debug.Log("Lean Vec:" + leanVec);

            if(leanVec != Vector3.zero)
                RopeLean(Vector3.Normalize(leanVec) * leanAmount);

            RopeClimb(climbVec * climbAmount);



            // Rotate based on mouse look
            {
                float lookVec = cameraController.lookVec.x;
                float sensitivityRotate = Mathf.Abs(cameraController.cameraTurn / cameraController.maxTurnAngle);
                sensitivityRotate = sensitivityRotate * sensitivityRotate;

                float turnAmount = lookVec * sensitivityRotate;

                RopeRotateOn(-turnAmount * OnRope.rotateSpeed * dt);
            }
        }
        
    }
    
    void RopeClimb(float amountUp)
    {
        OnRope.distUpRope = Mathf.Clamp(
            amountUp + OnRope.distUpRope,
            0.0f,
            OnRope.rope.GetPath().PathLength);
    }
    void RopeRotateOn(float amountRight)
    {
        OnRope.angleOnRope += amountRight;
    }
    void RopePump(float amountUp)
    {
        var epsilon = 0.00001f;
        var oldDist = OnRope.distPumpUp;
        OnRope.distPumpUp = Mathf.Clamp(OnRope.distPumpUp + amountUp, -OnRope.maxPumpDownDist, OnRope.maxPumpUpDist);

        // Done with the pump
        if (OnRope.distPumpUp + epsilon > OnRope.maxPumpUpDist)
            return;
        if (OnRope.distPumpUp - epsilon < -OnRope.maxPumpDownDist)
            return;

        if (oldDist != OnRope.distFromRope)
            OnRope.rope.velocity += OnRope.rope.velocity * (amountUp * OnRope.pumpAcceleration);
    }
    void RopeLean(Vector3 amountVec)
    {
        OnRope.rope.velocity += transform.rotation * amountVec;
        //Debug.DrawLine(transform.position, transform.position + amountVec * 20.0f, Color.grey);
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

        var distOnPath = Mathf.Clamp(ropeLength - (OnRope.distUpRope), 0.0f, ropeLength);
        //var velocity = rope.VelocityAtLength(OnRope.distUpRope);

        // update Character Position
        var AngleFromDown = Quaternion.FromToRotation(Vector3.down, ropeVecNorm);
        var angularRotationOnRope = Quaternion.AngleAxis(OnRope.angleOnRope, ropeVecNorm) * AngleFromDown;
        var positionOnRope = ropePath.PointAlongPath(distOnPath);

        // @ TODO: Add charater offset!
        transform.position = positionOnRope +                                   // Position on rope
            (angularRotationOnRope * -Vector3.forward * OnRope.distFromRope) +  // set offset out from rope based on rotation
            (ropeVecNorm * -OnRope.distPumpUp);                                  // vertical offset from pumping
        
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
