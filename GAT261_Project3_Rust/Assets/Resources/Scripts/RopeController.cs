using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RopeChange
{
    public RopeController controller;
}

public class RopeController : MonoBehaviour
{
    private FFPath path; // rope is exactly 2 points



    public float length = 25.0f;
    public float mass = 1.5f;
    public float springForce = 45.0f;

    public Vector3 velocity = Vector3.zero;
    public float speed{ get { return velocity.magnitude; } }
    public float friction = 0.05f;



	// Use this for initialization
	void Start ()
    {
        path = GetComponent<FFPath>();
        Debug.Assert(path.points.Length == 2);
	}
	
    void FixedUpdate()
    {
        UpdateRopeMovement(Time.fixedDeltaTime);
    }


    void UpdateRopeMovement(float dt)
    {
        var epsilon = 0.005f;
        var ropeVec = path.points[1] - path.points[0];
        var ropeVecNorm = Vector3.Normalize(ropeVec);

        var down = Vector3.Normalize(Physics.gravity);



        if (ropeVec.magnitude + epsilon >= length &&
            Vector3.Dot(ropeVec, down) > 0.0f) //  rope is tight (Orbiting)
        {
            var rightVec = -Vector3.Cross(ropeVecNorm, down);
            var vecAlongEdgeOfSphere = Vector3.Normalize(Vector3.Cross(ropeVecNorm, rightVec));

            var percentAlongLine = Vector3.Dot(vecAlongEdgeOfSphere, down); // may be off!!!

            Debug.DrawLine(path.PositionAtPoint(1), path.PositionAtPoint(1) + vecAlongEdgeOfSphere, Color.green);

            // cast velocity along circumfrance of sphere
            velocity += dt * percentAlongLine * Physics.gravity.magnitude * vecAlongEdgeOfSphere;
        }
        else // rope is slack (falling straight down)
        {
            velocity += dt * Physics.gravity;
        }
        
        // Apply springy nature of rope
        if(ropeVec.magnitude >= length)
        {
            var rightVec = -Vector3.Cross(ropeVecNorm, down);
            var vecAlongEdgeOfSphere = Vector3.Normalize(Vector3.Cross(ropeVecNorm, rightVec));
            var delta = ropeVec.magnitude - length;

            // remove all velocity not inline with currenct direction
            velocity = Vector3.ProjectOnPlane(velocity, Vector3.Normalize(ropeVecNorm));

            // Apply spring force
            velocity += dt * delta * (springForce / 100.0f) * -ropeVecNorm;

            // Set Rope Position to match radius
            path.points[1] = Vector3.Normalize(ropeVecNorm) * length;
        }

        // apply friction
        //velocity = Vector3.Lerp(velocity, Vector3.zero, friction * dt);

        // Apply velocity
        path.points[1] += dt * velocity;

        // make sure rope is still within bounds
        ropeVec = path.points[1] - path.points[0];
        ropeVecNorm = Vector3.Normalize(ropeVec);
        if (ropeVec.magnitude >= length)
        {
            // Set Rope Position to match radius when its beyond the point
            path.points[1] = Vector3.Normalize(ropeVecNorm) * length;
        }


        Debug.DrawLine(path.PositionAtPoint(1), path.PositionAtPoint(1) + velocity, Color.red);
    }

    public float distBetweenRopeVisuals = 0.1f;
    private List<Transform> visualElements = new List<Transform>();

    void UpdateRopeVisuals()
    {
        // Draw visuals along rope

        float distAlongPath = 0;
        int indexElement = 0;
        do
        {
            Transform element;
            if (indexElement > visualElements.Count)
                AddVisualElement();

            element = visualElements[indexElement];

            element.position = path.PointAlongPath(distAlongPath);


            distAlongPath += distBetweenRopeVisuals;
        } while (distAlongPath <= path.PathLength);
    }

    void AddVisualElement()
    {
        var element = FFResource.Load_Prefab("RopeVisualElement").transform;
        visualElements.Add(element);
    }

    void SendUpdateEvent()
    {
        RopeChange rc;
        rc.controller = this;
        FFMessageBoard<RopeChange>.SendToLocal(rc, gameObject);
    }


}
