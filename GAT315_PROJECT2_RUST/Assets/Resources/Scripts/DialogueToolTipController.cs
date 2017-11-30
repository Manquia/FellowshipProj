using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueToolTipController : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        FFMessageBoard<OrateEvent>.Connect(OnOrateEvent, gameObject);
        FFMessageBoard<FastForwardOrate>.Connect(OnFastForwardOrate, gameObject);
        FFMessageBoard<DisableOration>.Connect(OnDiableOration, gameObject);

		
	}
    private void OnDestroy()
    {
        FFMessageBoard<OrateEvent>.Disconnect(OnOrateEvent, gameObject);
        FFMessageBoard<FastForwardOrate>.Disconnect(OnFastForwardOrate, gameObject);
        FFMessageBoard<DisableOration>.Disconnect(OnDiableOration, gameObject);
    }

    bool active = true;
    float timeToShowFastForward = 0;
    private void FixedUpdate()
    {
        if (!active) return;

        if(timeToShowFastForward > 0.0f)
        {
            timeToShowFastForward = Mathf.Max(0.0f, timeToShowFastForward - Time.fixedDeltaTime);

            GetComponent<ToolTip>().toolTipTitle = "--->";
        }
    }

    private void OnOrateEvent(OrateEvent e)
    {
        timeToShowFastForward += e.time;
    }

    private void OnFastForwardOrate(FastForwardOrate e)
    {
        timeToShowFastForward -= e.time;
    }

    private void OnDiableOration(DisableOration e)
    {
        active = false;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public struct OrateEvent
    {
        public float time;
        public DialogManager.OratorNames orator;
        public string text;
        public QueuedDialog.Type type;
    }
    public struct FastForwardOrate
    {
        public float time;
    }
    public struct DisableOration
    {
    }
}
