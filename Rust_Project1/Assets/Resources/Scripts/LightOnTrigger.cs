using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightOnTrigger : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        FFMessageBoard<TriggerObject>.Connect(OnTriggerObject, gameObject);
    }
    void OnDestroy()
    {
        FFMessageBoard<TriggerObject>.Disconnect(OnTriggerObject, gameObject);
    }
    
    private void OnTriggerObject(TriggerObject e)
    {
        var light = GetComponent<Light>();
        light.enabled = true;

        // TODO make this cooler!!

    }
}
