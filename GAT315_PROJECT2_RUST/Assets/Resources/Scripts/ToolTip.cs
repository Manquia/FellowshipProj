using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTip : MonoBehaviour, Interactable
{
    public string toolTipTitle;
    public Vector3 localOffset;
    //public string toolTipText;


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
            toolTip.SetActive(false);
        }

        // Setup Text of tooltip
        {
            var background = toolTip.transform.Find("Background");
            var title = background.Find("Title");
            //var text = background.Find("Text");

            //text.GetComponent<UnityEngine.UI.Text>().text = toolTipText;
            title.GetComponent<UnityEngine.UI.Text>().text = toolTipTitle;
        }

        // Find player to billboard
        {
            playerCamera = GameObject.Find("Camera").transform;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(toolTip.activeSelf)
        {
            toolTip.transform.LookAt(playerCamera, Vector3.up);
            toolTip.transform.Rotate(0, 180, 0);
            var vecToCamera = toolTip.transform.position - playerCamera.position;
            var distToCamera = vecToCamera.magnitude;

            toolTip.transform.localScale = new Vector3(1 + distToCamera, 1 + distToCamera, 1 + distToCamera);

            var background = toolTip.transform.Find("Background");
            var title = background.Find("Title");
            title.GetComponent<UnityEngine.UI.Text>().text = toolTipTitle;
        }
    }

    public void MouseOver(bool active)
    {
        toolTip.SetActive(active);
    }

    public void Use() // @TODO: Make object temporaryally disappear?
    {
    }
}
