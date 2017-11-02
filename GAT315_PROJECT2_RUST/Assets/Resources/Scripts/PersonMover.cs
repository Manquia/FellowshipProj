using UnityEngine;

public class PersonMover : FFComponent
{ 
    FFAction.ActionSequence seq;

    public float distAlongPath = 0;

    [Range(0.1f, 100.0f)]
    public float movementSpeed;

    public int startingIndex = 0;
    int currentPointIndex = 0;
    public FFPath PathToFollow;

    // Use this for initialization
    void Awake()
    {
        currentPointIndex = startingIndex;
        seq = action.Sequence();
    }

    public void Move(int index)
    {
        seq.ClearSequence();
        int currIndex = currentPointIndex;
        currentPointIndex = index;

        if (index >= currIndex)// move forward
            MoveForward();
        else
            MoveBackward();
    }

    // Move forward state
    void MoveForward()
    {
        float lengthToNextPoint = PathToFollow.LengthAlongPathToPoint(currentPointIndex);
        if (distAlongPath >= lengthToNextPoint) // reached next point
        {
            transform.position = PathToFollow.PointAlongPath(lengthToNextPoint); // goto point
            return;
        }
        else // keep moving along path
        {
            transform.position = PathToFollow.PointAlongPath(distAlongPath);
        }

        distAlongPath += Time.deltaTime * movementSpeed;
        seq.Sync();
        seq.Call(MoveForward);
    }

    // Move Backward state
    void MoveBackward()
    {
        float lengthToPrevPoint = PathToFollow.LengthAlongPathToPoint(currentPointIndex);
        if (distAlongPath <= lengthToPrevPoint) // reached begining
        {
            transform.position = PathToFollow.PointAlongPath(lengthToPrevPoint); // goto point
            return;
        }
        else // keep moving along path
        {
            transform.position = PathToFollow.PointAlongPath(distAlongPath);
        }

        distAlongPath -= Time.deltaTime * movementSpeed;
        seq.Sync();
        seq.Call(MoveBackward);
    }
    
}
