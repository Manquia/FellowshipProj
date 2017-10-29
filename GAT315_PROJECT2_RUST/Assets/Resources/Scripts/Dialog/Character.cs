using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


#region Events
public class QueryLight
{
    public QueryLight(Vector3 inPoint)
    {
        in_point = inPoint;
    }

    public Vector3 in_point;
    public float out_intensity = 0.0f;
}
public class QueryParty
{
    public QueryParty(string inCharacterName)
    {
        in_characterName = inCharacterName;
        out_character = null;
    }
    public string in_characterName;
    public Character out_character;
}



public struct Transformation
{
    public enum Type
    {
        Human,
        Grue,
    }
    public Type transformationType;
    public Character character;
}

public struct EnterParty
{
    public Character character;
}
public struct LeaveParty
{
    public Character character;
}
public struct EnterArea
{
    public Transform area;
}
public struct LeaveArea
{
    public Transform area;
}



public struct CustomEvent
{
    public string tag;
}
#endregion


public class Character : FFComponent
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
    
    // Update is called once per frame
    void Update () {
		
	}
}
