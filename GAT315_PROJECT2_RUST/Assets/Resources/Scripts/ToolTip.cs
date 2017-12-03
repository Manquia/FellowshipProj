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


        // Make object disabled when you start
        gameObject.SetActive(false);
    }


    void OnDestoy()
    {
    }

    public void ShowTooltip(bool active)
    {
        toolTip.SetActive(active);
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
                break;
        }

        // Face the player
        if (toolTip.activeSelf)
        {
            toolTip.transform.LookAt(playerCamera, Vector3.up);
            toolTip.transform.Rotate(0, 180, 0);
            var vecToCamera = toolTip.transform.position - playerCamera.position;
            var distToCamera = vecToCamera.magnitude;

            toolTip.transform.localScale = new Vector3(1 + distToCamera, 1 + distToCamera, 1 + distToCamera);
        }
    }
}
