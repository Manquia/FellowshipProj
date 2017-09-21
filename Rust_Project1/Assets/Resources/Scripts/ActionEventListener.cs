using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActionEventListener : MonoBehaviour
{
    public string[] events;
    private bool[] status;

    bool actionFinished = false;

	// Use this for initialization
	void Start ()
    {
        status = new bool[events.Length];

        for (int i = 0; i < events.Length; ++i)
        {
            var box = FFMessageBoard<CustomDialogOn>.Box(events[i]);
            box.Connect(OnCustomEvent);
        }
	}

    private void OnCustomEvent(CustomDialogOn e)
    {
        int index = 0;
        for(int i = 0; i < events.Length; ++i)
        {
            if (e.tag == events[i])
                break;

            ++index;
        }
        

    }

    // Update is called once per frame
    void Update ()
    {
        if (actionFinished == true)
            return;

        CheckEvents();
	}

    void CheckEvents()
    {


    }
}
