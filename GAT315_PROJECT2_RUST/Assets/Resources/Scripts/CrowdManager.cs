using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SwapCrowd
{
}

public class CrowdManager : MonoBehaviour {


    int totalCrowdCharacter = 60;
    public int maxCrowdSize
    {
        get { return totalCrowdCharacter / 2; }
    }


    void Awake()
    {
        InitCrowd();
    }

	// Use this for initialization
	void Start ()
    {
        FFMessage<EndCharacterHearing>.Connect(OnEndCharacterHearing);
    }
    void OnDestroy()
    {
        FFMessage<EndCharacterHearing>.Connect(OnEndCharacterHearing);
    }

    private void OnEndCharacterHearing(EndCharacterHearing e)
    {
        // Handed directly b/c of returning number of people in crowd
        //SwapCrowd();
    }

    // Update is called once per frame
    void Update ()
    {
		
	}

    Dictionary<int, bool> seatTaken = new Dictionary<int, bool>();

    void InitCrowd()
    {
        var crowdCharacter = FFResource.Load_Prefab("CrowdCharacter");
        for (int i = 0; i < totalCrowdCharacter; ++i)
        {
            var crowdPerson = Instantiate(crowdCharacter).transform;
            waitingPeople.Add(crowdPerson);

            var scale = UnityEngine.Random.Range(0.6f, 1.05f);

            // set position out of player sight
            crowdPerson.position = new Vector3(0.0f, -100f, 0.0f);
            crowdPerson.localScale = new Vector3(scale, scale, scale);

            // Set offset of position to match scale
            var mover = crowdPerson.GetComponent<PersonMover>();
            mover.offset = new Vector3(
                UnityEngine.Random.Range(-0.1f, 0.1f),
                UnityEngine.Random.Range(-0.1f, 0.1f) - (1 - scale) * 0.5f,
                UnityEngine.Random.Range(0.1f, 0.1f));

            // Set color to be an off tint of white
            var meshRend = crowdPerson.GetComponent<MeshRenderer>();
            var newColor = Instantiate(meshRend.material);
            newColor.color = new Color(
                UnityEngine.Random.Range(0.95f, 1.0f),
                UnityEngine.Random.Range(0.95f, 1.0f),
                UnityEngine.Random.Range(0.95f, 1.0f), 1.0f);         
            meshRend.material = newColor;

            // @ TODO customize crowd a bit by color or somethign...
        }

        for (int i = 0; i < 6; ++i)
        {
            crowdPaths.Add(transform.GetChild(i).GetComponent<FFPath>());
        }
    }
    
    List<FFPath> crowdPaths = new List<FFPath>();
    List<Transform> seatedPeople  = new List<Transform>();
    List<Transform> waitingPeople = new List<Transform>();

    public int SwapCrowd()
    {
        // Have all occupied seats leave
        foreach(var person in seatedPeople)
        {
            var seatedInfo = person.GetComponent<SeatedInformation>();

            SendPersonOut(person);
            LeaveSeat(seatedInfo.seatNumber);
        }
        seatedPeople.Clear();


        // Send in new crowd
        var numberInCrownd = 
            UnityEngine.Random.Range(0, 15) +
            UnityEngine.Random.Range(-2, 14);

        for (int i = 0; i < numberInCrownd && waitingPeople.Count > 0 && SeatsAreAvailable() > 4; ++i)
        {
            // Find an open seat
            int randomSeatNumber;
            do
            {
                randomSeatNumber = UnityEngine.Random.Range(0, 29);
            } while (SeatOccupied(randomSeatNumber));

            OccupySeat(randomSeatNumber);

            int pathIndex = randomSeatNumber / 5;
            int pointIndex = (randomSeatNumber % 5) +
                3; // points offset into path

            SendPersonIn(
                waitingPeople[waitingPeople.Count - 1],
                crowdPaths[pathIndex],
                pointIndex,
                randomSeatNumber);

            waitingPeople.RemoveAt(waitingPeople.Count - 1);
        }

        return numberInCrownd;
    }

    void OnCrowdPersonExitCourt(PersonFinishedMoving e)
    {
        FFMessageBoard<PersonFinishedMoving>.Disconnect(OnCrowdPersonExitCourt, e.trans.gameObject);
        waitingPeople.Add(e.trans);
    }

    void OnCrowdPersonReachedSeat(PersonFinishedMoving e)
    {
        FFMessageBoard<PersonFinishedMoving>.Disconnect(OnCrowdPersonReachedSeat, e.trans.gameObject);
        seatedPeople.Add(e.trans);
    }

    void SendPersonOut(Transform person)
    {
        var mover = person.GetComponent<PersonMover>();
        mover.Move(0);

        var randomSpeed = UnityEngine.Random.Range(0.8f, 1.0f);

        mover.movementSpeed = randomSpeed;
        FFMessageBoard<PersonFinishedMoving>.Connect(OnCrowdPersonExitCourt, person.gameObject);
    }
    
    
    void SendPersonIn(Transform person, FFPath path, int index, int seatNumber)
    {
        var mover = person.GetComponent<PersonMover>();
        var seatedInfo = person.GetComponent<SeatedInformation>();

        seatedInfo.seatNumber = seatNumber;
        seatedInfo.pathIndex = index;
        seatedInfo.path = path;

        var randomSpeed = UnityEngine.Random.Range(0.8f, 1.9f);

        mover.movementSpeed = randomSpeed;
        mover.PathToFollow = path;
        mover.Move(index);
        FFMessageBoard<PersonFinishedMoving>.Connect(OnCrowdPersonReachedSeat, person.gameObject);
    }

    int SeatsAreAvailable()
    {
        int counter = maxCrowdSize;
        foreach (var seat in seatTaken)
        {
            if (seat.Value == true)
                --counter;
        }

        return counter;
    }
    bool SeatOccupied(int seatNumber)
    {
        if(seatTaken.ContainsKey(seatNumber))
        {
            return seatTaken[seatNumber];
        }
        return false;
    }
    void OccupySeat(int seatNumber)
    {
        if (seatTaken.ContainsKey(seatNumber))
        {
            seatTaken[seatNumber] = true;
        }
        else
        {
            seatTaken.Add(seatNumber, true);
        }
    }
    void LeaveSeat(int seatNumber)
    {
        if (seatTaken.ContainsKey(seatNumber))
        {
            seatTaken[seatNumber] = false;
        }
        else
        {
            seatTaken.Add(seatNumber, false);
        }
    }
}
