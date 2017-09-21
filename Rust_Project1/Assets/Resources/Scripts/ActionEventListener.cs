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
            TriggerObject to;
            FFMessageBoard<TriggerObject>.SendToLocal(to, gameObject);
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
