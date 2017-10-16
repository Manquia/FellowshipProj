using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct TriggerObject
{
}

public class ActionEventListener : MonoBehaviour
{
    public enum Type
    {
        Single,
        SinglePerPress,
        Repeats,
    }
    public Type type = Type.Single;
    public Trinary OverrideTriggers = Trinary.True;
    public string[] events;
    private int[] status;

    bool actionFinished = false;

	// Use this for initialization
	void Start ()
    {
        status = new int[events.Length];

        for (int i = 0; i < events.Length; ++i)
        {
            var box = FFMessageBoard<CustomEventOn>.Box(events[i]);
            box.Connect(OnCustomEventOn);
        }

        for (int i = 0; i < events.Length; ++i)
        {
            var box = FFMessageBoard<CustomEventOff>.Box(events[i]);
            box.Connect(OnCustomEventOff);
        }
    }
    void OnDestroy()
    {
        for (int i = 0; i < events.Length; ++i)
        {
            var box = FFMessageBoard<CustomEventOn>.Box(events[i]);
            box.Disconnect(OnCustomEventOn);
        }

        for (int i = 0; i < events.Length; ++i)
        {
            var box = FFMessageBoard<CustomEventOff>.Box(events[i]);
            box.Disconnect(OnCustomEventOff);
        }
    }

    private void OnCustomEventOn(CustomEventOn e)
    {
        for(int i = 0; i < events.Length; ++i)
        {
            if (e.tag == events[i])
            {
                // for single_Pre_Press we change the sign and count up to 0 for off untill we are 0 and then we can turn it on again
                if (status[i] < 0)
                    --status[i];
                else // status[i] >= 0
                    ++status[i];


                break;
            }
        }
        UpdateEventListener();
    }
    private void OnCustomEventOff(CustomEventOff e)
    {
        for (int i = 0; i < events.Length; ++i)
        {
            if (e.tag == events[i])
            {
                // for single_Pre_Press we change the sign and count up to 0 for off untill we are 0 and then we can turn it off
                if (status[i] < 0) 
                    ++status[i];
                else// status[i] >= 0
                    --status[i];


                break;
            }
        }
        UpdateEventListener();
    }

    void FixedUpdate()
    {
        // repeating types will be able to sent trigger many times once the action is completed
        if (type == Type.Repeats)
        {
            //Debug.Log("Repeats ActionEventListener");
            UpdateEventListener();
        }
    }

    // Update is called once per frame
    void UpdateEventListener ()
    {
        if (actionFinished == true &&
            type == Type.Single)
            return;

        if(CheckEvents())
        {
            actionFinished = true;

            switch (OverrideTriggers)
            {
                case Trinary.True:
                    SendOverride(TriggerArea.State.OVERRIDE_ON);
                    break;
                case Trinary.False:
                    SendOverride(TriggerArea.State.OVERRIDE_OFF);
                    break;
                case Trinary.Null:
                    break;
            }
            
            TriggerObject to;
            FFMessageBoard<TriggerObject>.SendToLocal(to, gameObject);

            // Toggle single switches to negative and then adds when below for changed in events
            if (type == Type.SinglePerPress)
            {
                for (int i = 0; i < status.Length; ++i)
                {
                    status[i] = -status[i];
                }
            }
        }
    }

    void SendOverride(TriggerArea.State state)
    {
        TriggerAreaOverride tao;
        tao.newState = state;
        tao.tag = "";

        foreach(var name in events)
        {
            tao.tag = name;
            FFMessageBoard<TriggerAreaOverride>.Box(name).SendToLocal(tao);
        }
    }

    bool CheckEvents()
    {
        for(int i = 0; i < status.Length; ++i)
        {
            if (status[i] <= 0)
                return false;
        }
        return true;
    }
}
