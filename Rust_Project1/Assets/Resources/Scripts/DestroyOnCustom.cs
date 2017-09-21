using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCustom : MonoBehaviour {
    
    // Use this for initialization
    void Start ()
    {
        FFMessageBoard<TriggerObject>.Connect(OnTriggerObject, gameObject);
	}
    void OnDestroy()
    {
        FFMessageBoard<TriggerObject>.Disconnect(OnTriggerObject, gameObject);
    }

    void OnTriggerObject(TriggerObject e)
    {
        Destroy(gameObject);
    }
}
