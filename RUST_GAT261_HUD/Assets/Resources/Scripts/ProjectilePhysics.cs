using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePhysics : FFComponent
{
    public Vector3 windVector;
    public Vector3 gravityVector;
    
	public enum Effects
	{
		Wind = 1,
		Gravity = 2,
		Magnetic = 4,
	}
	
	public Vector3 ApplyEffects(Vector3 pos, Effects effects, float effectStrength)
	{
        return Vector3.zero;
	}



	public Vector3 WindEffect(Vector3 vec, float dt)
    {
        return vec + windVector * dt;
    }
    public Vector3 GravityEffect(Vector3 vec, float dt)
    {
        return vec + gravityVector * dt;
    }
}
