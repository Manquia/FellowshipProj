﻿using System.Collections;
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
    public enum Gender
    {
        Male,
        Female,
    }
    [Serializable]
    public struct Details
    {
        public string name;
        public DialogManager.OratorNames oratorMapping;
        public Gender gender;
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
        FFMessageBoard<Character.DialogueFinished>.Connect(OnDialogueFinished, gameObject);
        
        dialogManager = GameObject.Find("DialogManager").GetComponent<DialogManager>();
        Debug.Assert(dialogManager != null, "No Dialog manager found");

        DialogManager.OratorData data;
        data.details = details;
        data.trans = transform;
        dialogManager.SetOrator(data);

        FFMessageBoard<PersonFinishedMoving>.Connect(OnPersonFinishedMoving, gameObject);
         
        // @HACK Fixes offset bugs with the talk tooltip
        {
            var talkToolTip = transform.Find("TalkToolTip");
            talkToolTip.localPosition = new Vector3(0.0f, talkToolTip.localPosition.y, 0.0f);
        }


        ConfigureConfigureSpriteFace();
    }

    private void ConfigureConfigureSpriteFace()
    {
        if(mugshot != null)
        {
            var faceSprite = Instantiate(FFResource.Load_Prefab("CharacterFace"));
            faceSprite.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = mugshot;
            faceSprite.transform.SetParent(transform);
            faceSprite.transform.localPosition = new Vector3(0, 0.85f, 0.0f);
        }
    }

    private void OnPersonFinishedMoving(PersonFinishedMoving e)
    {
        FFMessageBoard<PersonFinishedMoving>.Disconnect(OnPersonFinishedMoving, gameObject);
        GetSpeechController().EnableTooltip();
    }

    void OnDestroy()
    {
        FFMessageBoard<BeginCharacterHearing>.Disconnect(OnBeginCharacterHearing, gameObject);
        FFMessageBoard<EndCharacterHearing>.Disconnect(OnEndCharacterHearing, gameObject);
        FFMessageBoard<DialogueFinished>.Disconnect(OnDialogueFinished, gameObject);
    }

    private void OnDialogueFinished(DialogueFinished e)
    {
        // Finished
        if (inTrialDialogIndex >= inTrialDialog.Length)
        {
            DisableOration disOrientation;
            FFMessageBoard<DisableOration>.SendToLocalToAllConnected(disOrientation, gameObject);
        }
        else // set reenable tooltip
        {
            GetSpeechController().EnableTooltip();
        }
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

    [System.NonSerialized] public int inTrialDialogIndex = 0;
    public CharacterDialog.Dialog.Echo[] inTrialDialog;
    public Crime crime;

    public GameObject witness;


    private void OnBeginCharacterHearing(BeginCharacterHearing e)
    {
    }

    private void OnEndCharacterHearing(EndCharacterHearing e)
    {
    }

    public void MouseOver(bool active)
    {
    }

    bool CapitalLetter(char character)
    {
        if (character > 64 && character < 91)
            return true;
        else
            return false;
    }

    public void DisableDialogue()
    {
        var speechController = transform.Find("SpeechController").GetComponent<SpeechController>();
        speechController.DisableTooltip();
        speechController.gameObject.SetActive(false);
        active = false;
    }
    public void EnableDialogue()
    {
        var speechController = transform.Find("SpeechController").GetComponent<SpeechController>();
        speechController.EnableTooltip();
        speechController.gameObject.SetActive(true);
        active = true;
    }


    private void FixedUpdate()
    {
        if (orateTimes.Count > 0)
            orateTimes[0] -= Time.deltaTime;
    }

    bool active = true;
    public List<float> orateTimes = new List<float>();
    public void Use()
    {
        if (!active)
            return;

        // no longer orating?
        if(orateTimes.Count > 0 && orateTimes[0] < 0.0f)
        {
            orateTimes.RemoveAt(0);
        }
        
        // If already talking, fast forward
        if (orateTimes.Count > 0)
        {
            float timeRemaining = orateTimes[0];
            dialogManager.TimeWarp(timeRemaining);
            orateTimes.RemoveAt(0);
            return;
        }


        // in Trial
        if (inTrialDialogIndex < inTrialDialog.Length)
        {
            //Debug.Log("InTrialDialog Queue");
            string text = "";

            if (details.name != null && details.name.Length > 1)
            {
                text += ">";

                text += details.name[0];
                for (int i = 1; i < details.name.Length; ++i)
                {
                    if (CapitalLetter(details.name[i]))
                    {
                        text += " ";
                    }
                    text += details.name[i];
                }

                text += "<" + "\n";
            }

            text += inTrialDialog[inTrialDialogIndex].text;

            float time = dialogManager.CharacterOrate(
                inTrialDialog[inTrialDialogIndex].orator,
                text,
                inTrialDialog[inTrialDialogIndex].type);

            orateTimes.Add(time);
            //Debug.Log("time To finish Dialog: " + time);

            // for 
            OrateEvent oe;
            oe.orator = inTrialDialog[inTrialDialogIndex].orator;
            oe.text = inTrialDialog[inTrialDialogIndex].text;
            oe.type = inTrialDialog[inTrialDialogIndex].type;
            oe.time = time;
            FFMessageBoard<OrateEvent>.SendToLocalToAllConnected(oe, gameObject);

            ++inTrialDialogIndex;
        }
        
    }

    public struct OrateEvent
    {
        public float time;
        public DialogManager.OratorNames orator;
        public string text;
        public QueuedDialog.Type type;
    }
    public struct DialogueFinished
    {
    }
    public struct DisableOration
    {
    }
}
