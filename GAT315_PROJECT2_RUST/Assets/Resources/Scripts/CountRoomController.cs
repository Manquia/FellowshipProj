using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct SendInNextCharacter
{
}
public struct SendOutLastCharacter
{
}

public struct FadeToNextWeek
{
    public int week;
}

public struct FadeToLevelTransition
{
    public string LevelName;
}

[Serializable]
public struct PersonIndex
{
    public int appearanceWeek;
    public string name;
    public int stage;
}

public class CountRoomController : MonoBehaviour {

    public PersonIndex[] characterStarting;

    Dictionary<int, List<PersonIndex>> people = new Dictionary<int, List<PersonIndex>>();

    int peopleIndex = 0;
    int weekIndex = 0;

    List<Transform> accusedCreated = new List<Transform>();
    List<Transform> witnessCreated = new List<Transform>();

    public FFPath accusedEnterPath;
    public FFPath accusedExitPath;
    public FFPath witnessEnterPath;
    public FFPath witnessExitPath;

    public static float JudgeApprovalRatting = 75.0f;
    public float JudgeFireRating = 55.0f;

	// Use this for initialization
	void Start ()
    {
        JudgeApprovalRatting = 75.0f;
        // Add starting Characters to people List
        foreach (var person in characterStarting)
        {
            if(people.ContainsKey(person.appearanceWeek) == false)
            {
                people.Add(person.appearanceWeek, new List<PersonIndex>());
            }
            people[person.appearanceWeek].Add(person);
        }

        
        FFMessage<SendInNextCharacter>.Connect(OnSendInNextCharacter);
        FFMessage<SendOutLastCharacter>.Connect(OnSendOutLastCharacter);
        FFMessage<PassSentence>.Connect(OnPassSentence);
	}
    void OnDestroy()
    {
        FFMessage<SendInNextCharacter>.Disconnect(OnSendInNextCharacter);
        FFMessage<SendOutLastCharacter>.Disconnect(OnSendOutLastCharacter);
        FFMessage<PassSentence>.Disconnect(OnPassSentence);
    }

    private void OnSendOutLastCharacter(SendOutLastCharacter e)
    {
        int inquiryCount = 0;
        int inquiriesMade = 0;

        // Send Last person OUT
        if (accusedCreated.Count > 0)
        {
            Transform personTrans = accusedCreated[accusedCreated.Count - 1];
            PersonMover personMover = personTrans.GetComponent<PersonMover>();
            Character personCharacter = personTrans.GetComponent<Character>();

            personMover.PathToFollow = accusedExitPath;
            personMover.distAlongPath = 0.0f;
            personMover.Move(accusedExitPath.points.Length - 1);

            inquiryCount += personCharacter.inTrialDialog.Length;
            inquiriesMade += personCharacter.inTrialDialogIndex;

            // Sent witness out of the court room
            if(personCharacter.witness != null)
            {
                GameObject witnessGO = personCharacter.witness;
                Transform witnessTrans = witnessGO.transform;
                PersonMover witnessMover = witnessTrans.GetComponent<PersonMover>();
                Character witnessCharacter = witnessGO.GetComponent<Character>();

                witnessMover.PathToFollow = witnessExitPath;
                witnessMover.distAlongPath = 0.0f;
                witnessMover.Move(accusedExitPath.points.Length - 1);

                inquiryCount += witnessCharacter.inTrialDialog.Length;
                inquiriesMade += witnessCharacter.inTrialDialogIndex;
            }
        }

        // Adjust approval rating changes
        {
            float percentageOfInquiries = 100.0f * ((float)inquiriesMade / (float)inquiryCount);

            // Shifting average
            float newApprovalrRating = ((JudgeApprovalRatting * 2) + percentageOfInquiries) / 3.0f;

            // @TODO: Make this info known to player so they know if they did well on the trial.
            JudgeApprovalRatting = newApprovalrRating;

            if(JudgeApprovalRatting < JudgeFireRating)
            {
                LoadFiredLevel();
            }
        }
    }

    void LoadFiredLevel()
    {
        FadeToLevelTransition fteg;
        fteg.LevelName = "FiredLevel";
        FFMessage<FadeToLevelTransition>.SendToLocal(fteg);
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
        pi.stage = e.sent.stage;
        people[appearanceWeek].Add(pi);
    }

    private void OnSendInNextCharacter(SendInNextCharacter e)
    {

        // Send Next Person In
        GameObject nextPerson = GetNextPerson();
        if (nextPerson != null)
        {
            Transform accusedTrans;
            PersonMover accusedMover;
            Character accusedCharacter;
            
            accusedTrans = nextPerson.transform;
            accusedCharacter = nextPerson.GetComponent<Character>();

            // Set starting Position
            accusedTrans.position = accusedEnterPath.PositionAtPoint(0);

            // Set path
            accusedMover = nextPerson.GetComponent<PersonMover>();
            accusedMover.PathToFollow = accusedEnterPath;
            accusedMover.Move(accusedEnterPath.points.Length - 1);
            accusedCreated.Add(accusedTrans);

            // Setup desk
            var judgeDesk = GameObject.Find("JudgeDesk").GetComponent<JudgeDesk>();
            judgeDesk.SetupDesk(accusedCharacter);

            // Create witness
            if (accusedTrans.GetComponent<Character>().witness != null)
            {
                GameObject witnessGO = Instantiate(accusedCharacter.witness);
                Transform witnessTrans = witnessGO.transform;
                PersonMover witnessMover = witnessGO.GetComponent<PersonMover>();

                // Set the GO to the in world character
                accusedCharacter.witness = witnessGO;
                
                // Set starting Position
                witnessTrans.position = witnessEnterPath.PositionAtPoint(0);

                // Set path
                witnessMover = nextPerson.GetComponent<PersonMover>();
                witnessMover.PathToFollow = witnessEnterPath;
                witnessMover.Move(witnessEnterPath.points.Length - 1);
                witnessCreated.Add(witnessTrans);
            }
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
                FadeToLevelTransition fteg;
                fteg.LevelName = "EndingLevel";
                FFMessage<FadeToLevelTransition>.SendToLocal(fteg);
            }
        }
        
    }
    

    GameObject GetNextPerson()
    {
        if (weekIndex == -1) return null;

        var curPeople = people[weekIndex];
        if(peopleIndex >= curPeople.Count)
		{
            return null;
		}

        string prefabName = "Characters/";
        // build prefab name
        {
            prefabName += curPeople[peopleIndex].name;
            prefabName += "[" + curPeople[peopleIndex].stage + "]";

			/*
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
			*/
        }

        Debug.Log("Name: " + prefabName);
        GameObject person = FFResource.Load_Prefab(prefabName);

		
        ++peopleIndex;
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