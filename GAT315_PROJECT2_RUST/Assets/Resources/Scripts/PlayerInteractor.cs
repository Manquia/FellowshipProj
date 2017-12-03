using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour {

    struct ObjectOverRecord
    {
        public Transform trans;
        public int overItemEnumeration;
    }

    public Transform relative;
    public Vector3 rayVector = Vector3.forward;

    List<ObjectOverRecord> overObjects = new List<ObjectOverRecord>();

	// Use this for initialization
	void Start ()
    {
		
	}
    public bool active = true;
    // Update is called once per frame
    int lastOverEnumeration = 0;
	void Update ()
    {
        if (!active) return;

        ++lastOverEnumeration;

        bool leftMouseDown = Input.GetMouseButtonUp(0);



        // Raycast
        var ray = new Ray(transform.position, transform.TransformVector(rayVector));
        int rayMask = LayerMask.GetMask("Default");
        Debug.DrawRay(ray.origin, ray.direction, Color.red); // debug
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, 100.0F, rayMask);

        //Mouse Over (true)
        foreach(var hit in raycastHits)
        {
            bool alreadyOver = false;
            // Already Over?
            for(int i = 0; i < overObjects.Count; ++i)
            {
                if(overObjects[i].trans == hit.transform)
                {
                    ObjectOverRecord oor;
                    oor.trans = hit.transform;
                    oor.overItemEnumeration = lastOverEnumeration;

                    overObjects[i] = oor;
                    alreadyOver = true;
                    break;
                }
            }

            if (alreadyOver)
            {
                continue;
            }
            else// We are over a new item!
            {
                var interactables = hit.transform.GetComponents<Interactable>();
                foreach (var inter in interactables)
                {
                    //Debug.Log("Over New Interactable! True");
                    inter.MouseOver(true);
                }

                ObjectOverRecord oor;
                oor.trans = hit.transform;
                oor.overItemEnumeration = lastOverEnumeration;
                overObjects.Add(oor);
            }
        }

        // Mouse over (False)
        for (int i = 0; i < overObjects.Count; ++i)
        {
            if (overObjects[i].overItemEnumeration != lastOverEnumeration)
            {
                var interactables = overObjects[i].trans.GetComponents<Interactable>();
                foreach (var inter in interactables)
                {
                    inter.MouseOver(false);
                }
                //Debug.Log("Over New Interactable! False");
                overObjects.RemoveAt(i);
            }
        }

        // clicked with mouse?
        if (leftMouseDown)
        {
            for (int i = 0; i < overObjects.Count; ++i)
            {
                var interactables = overObjects[i].trans.GetComponents<Interactable>();
                foreach (var inter in interactables)
                {
                    inter.Use();
                }
            }
        }
	}
}
