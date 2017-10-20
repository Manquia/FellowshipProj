using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct UpdatePlayer
{
	public float dt;
}

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour 
{
	
	Rigidbody rigid;
	
	enum UIState
	{
		Game,
		Weapons,
		Menu,
	}
	enum PlayerState
	{
		None = 0,
		Moveing = 1,
		Aiming = 2,
		Shooting = 4,
	}
	
	void Start () 
	{
		rigid = GetComponent<Rigidbody>();
		
		FFMessageBoard<UpdatePlayer>.Connect(OnUpdatePlayer, gameObject);
		
		// Get Dashes
		dashRoot = transform.Find("DashRoot");
		foreach(var child in dashRoot)
		{
			dashes.Add(child);
		}
	}
	
	void OnDestroy()
	{
		FFMessageBoard<UpdatePlayer>.Disconnect(OnUpdatePlayer, gameObject);
	}
	
	#region PlayerStateData
	private UIState uiState = UIState.Game;
	private PlayerState playerState = PlayerState.None;
	
	
	#endregion PlayerStateData
	
	
	
	#region RotationData
	public float moveDirection = 0.0f; // Move left (-1.0f), non (0.0f), right (+1.0f)
	public float lookRotation = 0.0f; //  Directly up (0.0f)
	public Quaternion LookQuaternion
	{
		get { return Quaternion.AngleAxis (lookRotation, Vector3.forward); }
	}
	
	private float rotationTickTime = 0.0f;
	public float rotationTickSpeedCurveTimeLimit = 1.6f;  
	public AnimationCurve rotationTickSpeedCurve;
	public float rotationTickSpeed = 75.0f;
	#endregion RotationData
	
	#region PoolData
	Transform dashRoot;
	List<Transform> dashes = new List<Transform>();
	#endregion PoolData
	
	void Update()
	{
		UpdatePlayer e;
		e.dt = Time.deltaTime;
		OnUpdatePlayer(e);
	}
	
	void OnUpdatePlayer(UpdatePlayer e)
	{
		if(playerState == PlayerState.Idle) // Probably shouldn't ever happen
		{
			Debug.Log("Updateing Idle Player");
			return;
		}
		
		UpdatePlayerInput(); // Get Input, Set PlayerState,
		
		
		
		
		{ // Debug Look direction
			var lookDirection = LookQuaternion * transform.up;
			
			Debug.DrawLine(transform.position, transform.position + lookDirection, Color.red);
		}
		
		
		// Draw Pridiction Line
		{
			var lookDirection = LookQuaternion * transform.up;
			var playerPos = transform.position;
			var lastPos = playerPos;
			for(int i = 0; i < dashes.Count; ++i)
			{
				var dashPosition = 
					lastPos +
					lookDirection;
					
				// Apply effectors
					
				
				
				
				lastPos = dashPosition;
			}
		}
		
		
		
		
		
		
		
		
		
		
		
	}
	
	void UpdatePlayerInput()
	{
		playerState = PlayerState.None;
		
		{// Enter HUD for weapon selection
			if(Input.GetKey(KeyCode.Tab))
			{
				playerState = PlayerState.HUD;
				return;
			}
		}
		
		{ // Get Movement Vector
			moveDirection = 0.0f;
			if(Input.GetKey(KeyCode.LeftArrow))
				moveDirection -= 1.0f;
			if(Input.GetKey(KeyCode.RightArrow))
				moveDirection += 1.0f;
			
			if(moveDirection != 0.0f)
			{
				playerState = PlayerState.Moveing;
				return;
			}
		}
		
		{ // Get Rotation Vector
			rotationTickTime += e.dt;
			float rotateVecter = 0.0f;
			if(Input.GetKey(KeyCode.UpArrow))
				rotateVecter -= 1.0f;
			if(Input.GetKey(KeyCode.DownArrow))
				rotateVecter += 1.0f;
			
			if(rotateVecter != 0.0f) // rotating
			{
				rotateVecter = 
					rotateVecter * 
					rotationTickSpeed *
					e.dt *
					rotationTickSpeedCurve.Evaluate(rotationTickTime / (rotationTickSpeedCurveTimeLimit + rotationTickTime));
					
				lookRotation += rotateVecter;
				lookRotation = Mathf.Clamp(lookRotation, -179.9f, 179.9f);
				playerState |= PlayerState.Aiming;
			}
			else // reset timer
			{
				rotationTickTime = 0.0f;	
			}
		}

		{ // Shooting?
			if(Input.GetKey(KeyCode.Space))
				playerState |= PlayerState.Shooting;
		}
	
	}



}





