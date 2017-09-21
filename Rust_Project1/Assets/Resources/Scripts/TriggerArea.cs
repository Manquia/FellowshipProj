using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    public DialogManager.OratorNames SpecifyPerson = DialogManager.OratorNames.None;
    public string tag;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {	
	}

    void OnTriggerEnter(Collider col)
    {
        var character = col.GetComponent<Character>();
        if (character != null &&
           (SpecifyPerson == DialogManager.OratorNames.None) /* || @TODO Add non specific character requirement*/)
        {
            CustomDialogOn cdo;
            cdo.tag = tag;
            var box = FFMessageBoard<CustomDialogOn>.Box(name);
            box.SendToLocal(cdo);
        }
    }

    void OnTriggerExit(Collider col)
    {
        var character = col.GetComponent<Character>();
        if (character != null &&
           (SpecifyPerson == DialogManager.OratorNames.None) /* || @TODO Add non specific character requirement*/)
        {
            CustomDialogOff cdo;
            cdo.tag = tag;
            var box = FFMessageBoard<CustomDialogOff>.Box(name);
            box.SendToLocal(cdo);
        }
    }
}
