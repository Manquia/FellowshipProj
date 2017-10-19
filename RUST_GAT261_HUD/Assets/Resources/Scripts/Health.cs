using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct DamageEvent
{
	public float delta;
	public Transform origin;
}

public class Health : MonoBehaviour {

	public float start;
	[HideInInspector]public float current;
	public float max;
	
	public bool invulnerable;
	
	
	// Use this for initialization
	void Start () 
	{
		current = start;
		
		FFMessageBoard<DamageEvent>.Connect(OnDamageEvent, gameObject);
	}
	void OnDestroy()
	{
		FFMessageBoard<DamageEvent>.Disconnect(OnDamageEvent, gameObject);
	}
	
	void OnDamageEvent(DamageEvent de)
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
