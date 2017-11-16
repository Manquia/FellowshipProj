using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


#region Events
public struct CustomEvent
{
    public string tag;
}
#endregion

public struct BeginCharacterHearing
{
}

public struct EndCharacterHearing
{
}

[Serializable]
public struct Sentence
{
    public int appearsWeeksLater;
    public int stage;
    public string text;
}

[Serializable]
public struct Crime
{
    public string[] charges;
    public string chargesNotes;
    public string characterNameAge;
    public string Investigation;

    public Sentence sent1;
    public Sentence sent2;
    public Sentence sent3;
}

public class Character : FFComponent, Interactable
{
    [Serializable]
    public struct Details
    {
        public string name;
        public DialogManager.OratorNames oratorMapping;
    }
    public Details details;
    
    public Transform GetSpeachRoot()
    {
        return transform.Find("SpeechController");
    }
    public SpeechController GetSpeechController()
    {
        return transform.Find("SpeechController").GetComponent<SpeechController>();
    }

    Transform toolTip;
    DialogManager dialogManager;
    void Start()
    {
        FFMessageBoard<BeginCharacterHearing>.Connect(OnBeginCharacterHearing, gameObject);
        FFMessageBoard<EndCharacterHearing>.Connect(OnEndCharacterHearing, gameObject);

        var judgeDesk = GameObject.Find("JudgeDesk").GetComponent<JudgeDesk>();
        dialogManager = GameObject.Find("DialogManager").GetComponent<DialogManager>();
        Debug.Assert(dialogManager != null, "No Dialog manager found");

        dialogManager.SetOrator(details.oratorMapping, transform);
        judgeDesk.SetupDesk(this);
    }
    

    void OnDestroy()
    {
        FFMessageBoard<BeginCharacterHearing>.Disconnect(OnBeginCharacterHearing, gameObject);
        FFMessageBoard<EndCharacterHearing>.Disconnect(OnEndCharacterHearing, gameObject);
    }
    
    // Update is called once per frame
    void Update ()
    {
	}
    

    void BecomePerson()
    {
    }

    public Sprite mugshot;
    public CharacterDialog.Dialog.Echo[] enterDialog;
    public CharacterDialog.Dialog.Echo[] talkDialog;
    public CharacterDialog.Dialog.Echo[] exitHardSentence;
    public CharacterDialog.Dialog.Echo[] exitSoftSentence;

    int inTrialDialogIndex = 0;
    public CharacterDialog.Dialog.Echo[] inTrialDialog;
    public Crime crime;


    private void OnBeginCharacterHearing(BeginCharacterHearing e)
    {
    }

    private void OnEndCharacterHearing(EndCharacterHearing e)
    {
    }

    public void MouseOver(bool active)
    {
        if(active)
        {


        }
        else
        {

        }
    }

    public void Use()
    {
        // in Trial
        if (inTrialDialogIndex < inTrialDialog.Length)
        {
            Debug.Log("InTrialDialog Queue");

            dialogManager.CharacterOrate(
                inTrialDialog[inTrialDialogIndex].orator,
                inTrialDialog[inTrialDialogIndex].text,
                inTrialDialog[inTrialDialogIndex].type);
            
            ++inTrialDialogIndex;
        }
    }
}
