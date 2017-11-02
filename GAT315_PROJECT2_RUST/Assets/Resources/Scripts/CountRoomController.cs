using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct SendInNextCharacter
{
}

public class CountRoomController : MonoBehaviour {

    public GameObject[] people;
    int peopleIndex = 0;
    List<Transform> createdPeople = new List<Transform>();

    public FFPath enterPath;
    public FFPath exitPath;

	// Use this for initialization
	void Start ()
    {
        FFMessage<SendInNextCharacter>.Connect(OnSendInNextCharacter);
	}
    void OnDestroy()
    {
        FFMessage<SendInNextCharacter>.Disconnect(OnSendInNextCharacter);
    }

    private void OnSendInNextCharacter(SendInNextCharacter e)
    {

        // Send Last person OUT
        if(createdPeople.Count > 0)
        {
            Transform personTrans = createdPeople[createdPeople.Count -1];
            GameObject person = personTrans.gameObject;
            PersonMover personMover = personTrans.GetComponent<PersonMover>();
            
            personMover.PathToFollow = exitPath;
            personMover.distAlongPath = 0.0f;
            personMover.Move(exitPath.points.Length - 1);
        }

        // Send Next Person In
        if(peopleIndex < people.Length)
        {
            GameObject person;
            Transform personTrans;
            PersonMover personMover;

            // Create
            person = Instantiate(people[peopleIndex]);
            personTrans = person.transform;

            // Set starting Position
            personTrans.position = enterPath.PositionAtPoint(0);
            // Set path
            personMover = person.GetComponent<PersonMover>();
            personMover.PathToFollow = enterPath;
            personMover.Move(enterPath.points.Length - 1);
            createdPeople.Add(personTrans);
        }
        else // Count is over!
        {


        }
        
        ++peopleIndex;
    }



    // Update is called once per frame
    void Update ()
    {
		
	}
}
