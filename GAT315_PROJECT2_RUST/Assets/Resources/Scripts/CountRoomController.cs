using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct SendInNextCharacter
{
}

public struct FadeToNextWeek
{
    public int week;
}

public struct FadeToEndGame
{
}

[Serializable]
public struct PersonIndex
{
    public int appearanceWeek;
    public string name;
    public int stage;
    public Sentence.Type sent;
}

public class CountRoomController : MonoBehaviour {

    public PersonIndex[] characterStarting;

    Dictionary<int, List<PersonIndex>> people = new Dictionary<int, List<PersonIndex>>();

    int peopleIndex = 0;
    int weekIndex = 1;

    List<Transform> createdPeople = new List<Transform>();

    public FFPath enterPath;
    public FFPath exitPath;

	// Use this for initialization
	void Start ()
    {

        // Add starting Characters to people List
        foreach(var person in characterStarting)
        {
            if(people.ContainsKey(person.appearanceWeek) == false)
            {
                people.Add(person.appearanceWeek, new List<PersonIndex>());
            }
            people[person.appearanceWeek].Add(person);
        }



        FFMessage<SendInNextCharacter>.Connect(OnSendInNextCharacter);
        FFMessage<PassSentence>.Connect(OnPassSentence);
	}
    void OnDestroy()
    {
        FFMessage<SendInNextCharacter>.Disconnect(OnSendInNextCharacter);
        FFMessage<PassSentence>.Disconnect(OnPassSentence);
    }

    private void OnPassSentence(PassSentence e)
    {
        int appearanceWeek = weekIndex + e.sent.appearsWeeksLater;
        
        if(people.ContainsKey(appearanceWeek) == false)
        {
            people.Add(appearanceWeek, new List<PersonIndex>());
        }
        PersonIndex pi;
        pi.appearanceWeek = appearanceWeek;
        pi.name = e.name;
        pi.sent = e.sent.type;
        pi.stage = e.sent.stage;
        people[appearanceWeek].Add(pi);
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
            if(GotoNextWeek())
            {
                Debug.Log("Entering Week: " + weekIndex);

                FadeToNextWeek ftnw;
                ftnw.week = weekIndex;
                FFMessage<FadeToNextWeek>.SendToLocal(ftnw);
            }
            else // Game is over!
            {
                weekIndex = -1; // invalid index becuase end of game!
                FadeToEndGame fteg;
                FFMessage<FadeToEndGame>.SendToLocal(fteg);

            }
        }
        
        ++peopleIndex;
    }
    

    GameObject GetNextPerson()
    {
        if (weekIndex == -1) return null;

        var curPeople = people[weekIndex];
        if(peopleIndex >= curPeople.Count)
            return null;

        string prefabName = "Characters/";
        // build prefab name
        {
            prefabName += curPeople[peopleIndex].name;
            prefabName += "[" + curPeople[peopleIndex].stage + "]";

            switch (curPeople[peopleIndex].sent)
            {
                case Sentence.Type.None:
                    break;
                case Sentence.Type.Hard:
                    prefabName += "[Hard]";
                    break;
                case Sentence.Type.Soft:
                    prefabName += "[Soft]";
                    break;
                case Sentence.Type.End:
                    break;
                default:
                    Debug.Assert(false, "Unhandled Sentence type of End");
                    break;
            }
        }

        Debug.Log("Name: " + prefabName);
        GameObject person = FFResource.Load_Prefab(prefabName);

        if (person == null)
            return null;
        else
            return Instantiate(person);
    }


    private bool GotoNextWeek()
    {
        //Debug.Assert(people[weekIndex].Count > peopleIndex, "Trying to goto next week when we haven't done all of the people");

        // get current max week
        int maxWeek = 0;
        foreach(var week in people)
        {
            maxWeek = Mathf.Max(maxWeek, week.Key);
        }
        
        do
        {
            ++weekIndex;
            if(people.ContainsKey(weekIndex))
            {
                break;
            }

        } while (weekIndex <= maxWeek);


        peopleIndex = 0;

        if (weekIndex > maxWeek)
            return false;
        else
            return true;
    }

    public void AddPerson(Sentence sent, Character character)
    {



    }


    // Update is called once per frame
    void Update ()
    {
		
	}
}