using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct TriggerObject
{
}

public class ActionEventListener : MonoBehaviour
{
    public bool continious = false;
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
                ++status[i];
                break;
            }
        }
    }
    private void OnCustomEventOff(CustomEventOff e)
    {
        for (int i = 0; i < events.Length; ++i)
        {
            if (e.tag == events[i])
            {
                --status[i];
                break;
            }
        }
    }


    // Update is called once per frame
    void Update ()
    {
        if (actionFinished == true &&
            !continious)
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
