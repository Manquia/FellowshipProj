using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateOnTrigger : MonoBehaviour {


    public Transform[] objectsToDeactivate;
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
        foreach (var obj in objectsToDeactivate)
        {
            obj.gameObject.SetActive(false);
        }

    }
}
