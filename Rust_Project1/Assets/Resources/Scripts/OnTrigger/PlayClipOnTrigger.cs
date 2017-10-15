using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayClipOnTrigger : MonoBehaviour {
    
    // Use this for initialization
    void Start()
    {
        FFMessageBoard<TriggerObject>.Connect(OnTriggerObject, gameObject);
    }
    void OnDestroy()
    {
        FFMessageBoard<TriggerObject>.Disconnect(OnTriggerObject, gameObject);
    }

    void OnTriggerObject(TriggerObject e)
    {
        var audioSrc = GetComponent<AudioSource>();
        audioSrc.Play();   
    }
}
