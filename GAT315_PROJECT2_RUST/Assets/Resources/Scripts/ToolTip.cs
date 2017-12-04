using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// DIALOGUE ONLY!!!!


public class ToolTip : MonoBehaviour
{
    public string toolTipTitle;
    public Vector3 localOffset;
    //public string toolTipText;

    public string toolTipIdle = ". . .";
    public string toolTipOver = "Talk";
    public string toolTipUseing = "Skip";

    public Vector3 IdleOffset = Vector3.zero;
    public Vector3 OverOffset = Vector3.zero;
    Vector3 usingOffset = new Vector3(0,-1.12f, -0.35f);
    
    //Vector3 Idlescale  = new Vector3(1.0f,1.0f,1.0f);
    //Vector3 Overscale  = new Vector3(1.0f,1.0f,1.0f);
    //Vector3 Usingscale = new Vector3(0.1f,0.1f,0.1f);

    public enum State
    {
        Idle,
        Over,
        Using,
    }
    public State state = State.Idle;

    GameObject toolTip;
    Transform playerCamera;

    // Use this for initialization
    void Start ()
    {
        // Make ToolTip prefab
        {
            var toolTipPrefab = FFResource.Load_Prefab("ToolTip");
            toolTip = Instantiate(toolTipPrefab);

            toolTip.transform.SetParent(transform);
            toolTip.transform.localPosition = localOffset;
        }

        // Find player to billboard
        {
            playerCamera = GameObject.Find("Camera").transform;
        }
        

        // Are we a character Dialog Tooltip?
        if (transform.parent.GetComponent<Character>() != null)
            gameObject.AddComponent<DialogueToolTipController>();
    }
    
    public void SetState(State s)
    {
        state = s;
        Update();
    }
    

    // Update is called once per frame
    void Update ()
    {
        // Update text based on state
        var background = toolTip.transform.Find("Background");
        var title = background.Find("Title");
        switch (state)
        {
            case State.Idle:
                if (toolTipIdle == null)
                {
                    toolTip.SetActive(false);
                }
                else
                {
                    toolTip.SetActive(true);
                    title.GetComponent<UnityEngine.UI.Text>().text = toolTipIdle;
                }
                toolTip.transform.localPosition = toolTip.transform.rotation * IdleOffset;
                //toolTip.transform.localScale = Idlescale;
                break;
            case State.Over:
                if (toolTipOver == null)
                {
                    toolTip.SetActive(false);
                }
                else
                {
                    toolTip.SetActive(true);
                    title.GetComponent<UnityEngine.UI.Text>().text = toolTipOver;
                }
                toolTip.transform.localPosition = toolTip.transform.rotation * OverOffset;
                //toolTip.transform.localScale = Overscale;
                break;
            case State.Using:
                if (toolTipUseing == null)
                {
                    toolTip.SetActive(false);
                }
                else
                {
                    toolTip.SetActive(true);
                    title.GetComponent<UnityEngine.UI.Text>().text = toolTipUseing;
                }
                toolTip.transform.localPosition = toolTip.transform.rotation * usingOffset;
                //toolTip.transform.localScale = Usingscale;
                break;
        }

        // Face the player
        if (toolTip.activeSelf)
        {
            var vecToCamera = toolTip.transform.position - playerCamera.position;
            var distToCamera = vecToCamera.magnitude;

            toolTip.transform.LookAt(transform.position + (vecToCamera * 3.0f), Vector3.up);
            toolTip.transform.localScale = new Vector3(1 + distToCamera, 1 + distToCamera, 1 + distToCamera);
        }
    }
}
