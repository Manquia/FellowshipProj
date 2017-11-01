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

struct BeginCharacterHearing
{

}

public class Character : FFComponent, Interactable
{
    [Serializable]
    public struct Details
    {
        public DialogManager.OratorNames person;
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
        FFMessageBoard<BeginCharacterHearing>.Connect(OnCharacterHearing, gameObject);
    }
    

    void OnDestroy()
    {
        FFMessageBoard<BeginCharacterHearing>.Disconnect(OnCharacterHearing, gameObject);
    }
    
    // Update is called once per frame
    void Update ()
    {
	}
    

    void BecomePerson()
    {

    }


    public string[] enterDialog;
    public string[] talkDialog;
    public string[] exitHardSentence;
    public string[] exitSoftSentence;


    private void OnCharacterHearing(BeginCharacterHearing e)
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
