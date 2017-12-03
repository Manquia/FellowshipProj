using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GavelTooltipController : MonoBehaviour, Interactable
{
    public void MouseOver(bool active)
    {
        var tooltip = GetComponent<ToolTip>();
        

        if(active)
        {
            tooltip.SetState(ToolTip.State.Over);
        }
        else
        {
            tooltip.SetState(ToolTip.State.Idle);
        }
    }

    public void Use()
    {
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
