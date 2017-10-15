using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnTrigger : MonoBehaviour {


    public Transform[] objectsToActivate;
    // Use this for initialization
    void Start()
    {
        FFMessageBoard<TriggerObject>.Connect(OnTriggerObject, gameObject);
    }
    void OnDestroy()
    {
        FFMessageBoard<TriggerObject>.Disconnect(OnTriggerObject, gameObject);
    }

    private void OnTriggerObject(TriggerObject e)
    {
        foreach(var obj in objectsToActivate)
        {
            obj.gameObject.SetActive(true);
        }

    }
}
