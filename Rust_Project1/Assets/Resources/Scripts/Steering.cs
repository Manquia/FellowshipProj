using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour {

    public float rotationSpeed = 1.0f;
    [Range(0.01f, 0.5f)] public float FacingWindow = 0.1f;
    public float maxSpeed = 10.0f;
    public float acceleration = 50.0f;
    public float targetRadius = 0.25f;
    public float slowingRadius = 2.5f;

    public bool debugDraw = true;

    public FFRef<Vector3> targetPoint;
    //public Transform targetRelativeTo;
    public Vector3 forceVector;




    // Use this for initialization
    void Start ()
    {
        targetPoint = new FFVar<Vector3>(transform.position);
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    [Range(0.01f, 0.99f)]public float WiskerWeight = 0.8f;
    [Range(0.01f, 0.99f)]public float TargetWeight = 0.4f;

    public void SetupTarget(Transform relativeTo, Vector3 worldPoint)
    {
        if (relativeTo != null)
        {
            Debug.Log("position relateive " + worldPoint);
            var localOffset = relativeTo.InverseTransformPoint(worldPoint);

            targetPoint = new FFRef<Vector3>(
                () => relativeTo.TransformPoint(localOffset),
                (v) => { relativeTo.position = v; });

        }
        else
        {
            Debug.Log("Not relative " + worldPoint);
            targetPoint = new FFVar<Vector3>(targetPoint);
        }
    }

    void FixedUpdate()
    {
        var rigid = GetComponent<Rigidbody>();
        var position = transform.position;
        var vecToTarget = targetPoint - position;
        //var vecToTargetNorm = Vector3.Normalize(vecToTarget); // @Cleanup
        var distToTarget = vecToTarget.magnitude;
        var forward = transform.forward;
        var right = transform.right;
        var up = transform.up;
        

        // Within target radius?
        if (distToTarget < targetRadius)
        {
            return;
        }
        

        // @Cleanup. we may want to use force for this. but maybe not!
        var forceApplied = Mathf.Min(acceleration, (distToTarget / slowingRadius) * acceleration);
        var forceVec = Vector3.zero;

        // Query Feelers
        {
            forceVec += GuideFeelers() * WiskerWeight;
            forceVec += GuideTarget() * TargetWeight;
            //@ TODO add slight wonder depending on dist from player
            //@ TODO Add slight swerving to create a curved path, potentially do 2-3 of these
            //  to create an interesting movement pattern.


            forceVec = Vector3.Normalize(new Vector3(forceVec.x, 0.0f, forceVec.z));
        }


        // Apply Force
        {
            rigid.AddForce(forceApplied * forceVec * Time.fixedDeltaTime, ForceMode.Impulse);
        }


        // Limit velocity to maxSpeed
        {
            var velocity = new Vector3(
                rigid.velocity.x,
                0.0f,
                rigid.velocity.z);
            if(velocity.magnitude > maxSpeed)
            {
                var velocityVecNorm = Vector3.Normalize(velocity);
                rigid.velocity = new Vector3(0.0f, rigid.velocity.y, 0.0f) + velocityVecNorm * maxSpeed;
            }
        }

        // Face movement direction
        
        if (rigid.velocity.magnitude > 0.01f)
        {
            var velocityAngle = Mathf.Rad2Deg * Mathf.Atan2(rigid.velocity.x, rigid.velocity.z);
            var dotVelAndFace = Vector3.Dot(Vector3.Normalize(rigid.velocity), transform.forward);
            
            if (dotVelAndFace < 0.0f || 1.0f - dotVelAndFace > FacingWindow)
            {
                var rotationSpeedModifier  = 1.0f - dotVelAndFace;

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.AngleAxis(velocityAngle, up),
                    Time.fixedDeltaTime * rotationSpeed * rotationSpeedModifier
                );
            }
        }



    }
    Vector3 GuideTarget()
    {
        Vector3 dir = targetPoint - transform.position;

        return Vector3.Normalize(dir);
    }
    Vector3 GuideFeelers()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 left = -transform.right;
        Vector3 up = transform.up;

        float goLeft  = 0.0f;
        float goRight = 0.0f;

        var ray = new Ray(transform.position, Vector3.zero);

        var rot15Left  = Quaternion.AngleAxis(-10.0f, up);
        var rot15Right = Quaternion.AngleAxis(10.0f, up);

        ray.direction = Vector3.Normalize(forward);
        ray.direction = Quaternion.AngleAxis(5.5f, up) * ray.direction;
        ray.direction = rot15Left * ray.direction; // rot left
        goLeft += WiskerRay(ray);
        ray.direction = rot15Left * ray.direction; // rot left
        goLeft += WiskerRay(ray);
        ray.direction = rot15Left * ray.direction; // rot left
        goLeft += WiskerRay(ray);
        ray.direction = rot15Left * ray.direction; // rot left
        goLeft += WiskerRay(ray);
        
        ray.direction = Vector3.Normalize(forward);
        ray.direction = Quaternion.AngleAxis(-5.5f, up) * ray.direction;
        ray.direction = rot15Right * ray.direction; // rot Right
        goRight += WiskerRay(ray);
        ray.direction = rot15Right * ray.direction; // rot Right
        goRight += WiskerRay(ray);
        ray.direction = rot15Right * ray.direction; // rot Right
        goRight += WiskerRay(ray);
        ray.direction = rot15Right * ray.direction; // rot Right
        goRight += WiskerRay(ray);
        
        return Vector3.Normalize(goRight * right + left * goLeft);
    }

    public float WiskerLength = 5.0f;
    float WiskerRay(Ray ray)
    {
        RaycastHit hit;

        Physics.Raycast(ray, out hit, WiskerLength);

        if(debugDraw)
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * WiskerLength);
        }
        return Physics.Raycast(ray, out hit) ? hit.distance : WiskerLength;
    }
}
