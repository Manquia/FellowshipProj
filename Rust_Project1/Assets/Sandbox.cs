using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandbox : FFComponent {

    //FFAction.ActionSequence seq;


	// Use this for initialization
	void Start ()
    {
        //seq = action.Sequence();
	}

    private void middle()
    {
        Debug.Log("Middle");
    }

    private void begin()
    {
        Debug.Log("Begin");
    }
    private void end()
    {
        Debug.Log("end");
    }

    // Update is called once per frame
    void Update () {
		
	}
}
