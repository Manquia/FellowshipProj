using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBase : FFComponent {

    public MenuState[] ActiveStates;

    protected void OnPushMenuState(PushMenuState e)
    {
        bool newStateIsActive = false;
        bool prevStateWasActive = false;

        for (int i = 0; i < ActiveStates.Length; ++i)
        {
            if (e.newState == ActiveStates[i]) // new state one of our states
            {
                newStateIsActive = true;
            }
            if (e.prevState == ActiveStates[i]) // new state one of our states
            {
                prevStateWasActive = true;
            }
        }

        if (newStateIsActive == true && prevStateWasActive == false)
        {
            Activate();
        }
        else if (newStateIsActive == false && prevStateWasActive == true)
        {
            Deactivate();
        }

    }
    protected void OnPopMenuState(PopMenuState e)
    {
        bool newStateIsActive = false;
        bool popedStateWasActive = false;

        for (int i = 0; i < ActiveStates.Length; ++i)
        {
            if (e.newState == ActiveStates[i]) // new state one of our states
            {
                newStateIsActive = true;
            }
            if (e.popedState == ActiveStates[i]) // new state one of our states
            {
                popedStateWasActive = true;
            }
        }

        if (newStateIsActive == true && popedStateWasActive == false)
        {
            Activate();
        }
        else if (newStateIsActive == false && popedStateWasActive == true)
        {
            Deactivate();
        }
    }



    public void ObjectActive(object active)
    {
        gameObject.SetActive((bool)active);
    }


    public virtual void Activate()
    {
        Debug.LogError("UI BASE ACTIVATE SHOULD NEVER BE CALLED");
    }
    public virtual void Deactivate()
    {
        Debug.LogError("UI BASE DEACTIVATE SHOULD NEVER BE CALLED");
    }

}
