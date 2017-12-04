using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueToolTipController : MonoBehaviour, Interactable
{

	// Use this for initialization
	void Start ()
    {
        FFMessageBoard<Character.OrateEvent>.Connect(OnOrateEvent, gameObject);
        FFMessageBoard<Character.DisableOration>.Connect(OnDiableOration, gameObject);
        FFMessageBoard<Character.DialogueFinished>.Connect(OnDialogueFinished, gameObject);
    }
    

    private void OnDestroy()
    {
        FFMessageBoard<Character.OrateEvent>.Disconnect(OnOrateEvent, gameObject);
        FFMessageBoard<Character.DisableOration>.Disconnect(OnDiableOration, gameObject);
        FFMessageBoard<Character.DialogueFinished>.Disconnect(OnDialogueFinished, gameObject);
    }

    private void OnOrateEvent(Character.OrateEvent e)
    {

    }

    private void OnDiableOration(Character.DisableOration e)
    {
        Debug.Log("DiableOration");
        OnDestroy(); // disconnect immediatly
        gameObject.SetActive(false);
    }

    private void OnDialogueFinished(Character.DialogueFinished e)
    {
        GetComponent<ToolTip>().SetState(ToolTip.State.Idle);
    }
    
    // Update is called once per frame
    void Update () {
		
	}

    public void MouseOver(bool active)
    {
        if(GetComponent<ToolTip>().state == ToolTip.State.Idle ||
            GetComponent<ToolTip>().state == ToolTip.State.Over)
        {
            if (active)
                GetComponent<ToolTip>().SetState(ToolTip.State.Over);
            else
                GetComponent<ToolTip>().SetState(ToolTip.State.Idle);
        }
    }

    public void Use()
    {
        GetComponent<ToolTip>().SetState(ToolTip.State.Using);
    }
    
}
