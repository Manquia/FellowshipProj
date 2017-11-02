using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct SendInNextCharacter
{
}

[Serializable]
public struct PersonIndex
{
    public enum FateType
    {
        None,
        Hard,
        Soft,
    }

    public string name;
    public int stage;
    public FateType sent;
}

public class CountRoomController : MonoBehaviour {

    public PersonIndex[] people;
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
            GameObject prevPerson = personTrans.gameObject;
            PersonMover personMover = personTrans.GetComponent<PersonMover>();
            
            personMover.PathToFollow = exitPath;
            personMover.distAlongPath = 0.0f;
            personMover.Move(exitPath.points.Length - 1);
        }

        // Send Next Person In
        GameObject nextPerson = GetNextPerson();
        if (nextPerson != null)
        {
            Transform personTrans;
            PersonMover personMover;
            
            personTrans = nextPerson.transform;

            // Set starting Position
            personTrans.position = enterPath.PositionAtPoint(0);

            // Set path
            personMover = nextPerson.GetComponent<PersonMover>();
            personMover.PathToFollow = enterPath;
            personMover.Move(enterPath.points.Length - 1);
            createdPeople.Add(personTrans);
        }
        else // Count is over!
        {


        }
        
        ++peopleIndex;
    }

    GameObject GetNextPerson()
    {
        if(peopleIndex >= people.Length)
            return null;

        string prefabName = "Characters/";
        // build prefab name
        {
            prefabName += "[" + people[peopleIndex].stage + "]";

            switch (people[peopleIndex].sent)
            {
                case PersonIndex.FateType.None:
                    break;
                case PersonIndex.FateType.Hard:
                    prefabName += "[Hard]";
                    break;
                case PersonIndex.FateType.Soft:
                    prefabName += "[Soft]";
                    break;
            }
        }


        GameObject person = FFResource.Load_Prefab(prefabName);
        return person;
    }



        // Update is called once per frame
        void Update ()
    {
		
	}
}