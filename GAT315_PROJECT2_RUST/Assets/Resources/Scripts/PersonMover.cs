using UnityEngine;

public struct PersonFinishedMoving
{
}

public class PersonMover : FFComponent
{ 
    FFAction.ActionSequence seq = null;

    public float distAlongPath = 0;

    float movementSpeed = 12.0f;

    public int startingIndex = 0;
    int currentPointIndex = 0;
    public FFPath PathToFollow;

    // Use this for initialization
    void Awake()
    {
        currentPointIndex = startingIndex;
    }

    public void Move(int index)
    {
        if (seq == null) seq = action.Sequence();

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
            PersonFinishedMoving pfm;
            FFMessageBoard<PersonFinishedMoving>.SendToLocal(pfm, gameObject);
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
            PersonFinishedMoving pfm;
            FFMessageBoard<PersonFinishedMoving>.SendToLocal(pfm, gameObject);
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
