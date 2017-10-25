using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct ChangeHealthEvent
{
	public float delta;
	public Transform origin;
}

public class Health : MonoBehaviour {

	public float start;
	public float current;
	public float max;
	
	public bool invulnerable;
	
	
	// Use this for initialization
	void Start () 
	{
		current = start;	
		FFMessageBoard<ChangeHealthEvent>.Connect(OnDamageEvent, gameObject);
	}
	void OnDestroy()
	{
		FFMessageBoard<ChangeHealthEvent>.Disconnect(OnDamageEvent, gameObject);
	}
	
	void OnDamageEvent(ChangeHealthEvent de)
	{
        if (invulnerable)
        {
            return;
        }
        else
        {
            current += de.delta;
        }
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
