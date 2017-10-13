using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSphere : MonoBehaviour {

    [HideInInspector]
    public int index;

    private void OnTriggerEnter(Collider other)
    {
        SetCheckpoint sc;
        sc.index = index;
        FFMessage<SetCheckpoint>.SendToLocal(sc);
        
    }
}
