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
    public enum Type
    {
        Soft,
        Hard,
    }

    public string text;
    public Type type;
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
        return transform.Find("SpeechRoot");
    }
    public SpeechController GetSpeechController()
    {
        return transform.Find("SpeechRoot").GetComponent<SpeechController>();
    }

    void Start()
    {
        FFMessageBoard<BeginCharacterHearing>.Connect(OnBeginCharacterHearing, gameObject);
        FFMessageBoard<EndCharacterHearing>.Connect(OnEndCharacterHearing, gameObject);
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
    

    public CharacterDialog.Dialog.Echo[] enterDialog;
    public CharacterDialog.Dialog.Echo[] talkDialog;
    public CharacterDialog.Dialog.Echo[] exitHardSentence;
    public CharacterDialog.Dialog.Echo[] exitSoftSentence;


    private void OnBeginCharacterHearing(BeginCharacterHearing e)
    {
    }

    private void OnEndCharacterHearing(EndCharacterHearing e)
    {
        throw new NotImplementedException();
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

    }
}
