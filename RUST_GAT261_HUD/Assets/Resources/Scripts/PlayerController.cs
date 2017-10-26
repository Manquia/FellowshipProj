using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct UpdateTurn
{
	public float dt;
}
public struct BeginTurn { }
public struct EndTurn { }

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour 
{
    ProjectilePhysics projectilePhysics;

    Rigidbody rigid;

    private GameplayController gameplayerController;
    [HideInInspector]public Transform targetRedical;
    [HideInInspector]public Transform playerRedical;
    [HideInInspector]public Transform healthBar;
    [HideInInspector]public Transform healthBarText;

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
        projectilePhysics = GameObject.Find("ProjectilePhysics").GetComponent<ProjectilePhysics>();

        FFMessageBoard<UpdateTurn>.Connect(OnUpdateTurn, gameObject);
        FFMessageBoard<BeginTurn>.Connect(OnBeginTurn, gameObject);
        FFMessageBoard<EndTurn>.Connect(OnEndTurn, gameObject);

        // Get Redicals
        targetRedical   = transform.Find("TargetRadical"); targetRedical.gameObject.SetActive(false);
        playerRedical   = transform.Find("PlayerRadical"); playerRedical.gameObject.SetActive(false);
        healthBar       = transform.Find("UI").Find("HealthBar");
        healthBarText   = transform.Find("UI").Find("HealthBarText");
        gameplayerController = GameObject.Find("GameplayController").GetComponent<GameplayController>();


        // Get Dashes
        dashRoot = transform.Find("DashRoot");
		foreach(Transform child in dashRoot)
		{
			dashes.Add(child);
		}
	}
	
	void OnDestroy()
	{
		FFMessageBoard<UpdateTurn>.Disconnect(OnUpdateTurn, gameObject);
        FFMessageBoard<BeginTurn>.Disconnect(OnBeginTurn, gameObject);
        FFMessageBoard<EndTurn>.Disconnect(OnEndTurn, gameObject);
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
    public Transform arrowDash;
	#endregion PoolData
	
    
    public float shootSpeed = 3.6f;
    public float minShootSpeed = 2.5f;
    public float MaxShootSpeed = 12.5f;

    float firingTimer = 0.0f;
    public float firingTime = 3.5f;

    public float distanceBetweenDashes = 0.1f;
    public float speedToDashRation = 5.0f;

    private void OnBeginTurn(BeginTurn e)
    {
        playerRedical.gameObject.SetActive(true);
        targetRedical.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
        healthBarText.gameObject.SetActive(false);
    }
    private void OnEndTurn(EndTurn e)
    {
        playerRedical.gameObject.SetActive(false);
        targetRedical.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(true);
        healthBarText.gameObject.SetActive(true);
    }

    void OnUpdateTurn(UpdateTurn e)
	{
        UpdateDemoActions();

		UpdatePlayerInput(e); // Get Input, Set PlayerState,

        // Debug Look direction
        {
            var lookDirection = LookQuaternion * transform.up;
			Debug.DrawLine(transform.position, transform.position + lookDirection, Color.red);
		}
        
		// Draw Pridiction Line
		{
            float simulationDT = Time.fixedDeltaTime * 0.5f;
            Vector3 gravity = Physics.gravity;

			var lookDirection = LookQuaternion * transform.up;
            var normLookDirection = Vector3.Normalize(lookDirection);
			var playerPos = transform.position;

            var lastVec = normLookDirection * shootSpeed;
			var lastPos = playerPos;

            // visableDashes = (shootSpeed * 0.5)^3
            var visableDashes = Mathf.Max(1.25f, shootSpeed * 0.35f); visableDashes *= visableDashes; visableDashes *= visableDashes;

            Vector3 offset = new Vector3(0.0f, 0.0f, -5.0f);
            
            int dashesToUse = Mathf.Min(dashes.Count, (int)(visableDashes * speedToDashRation));

            { // Set arrow dot as last item to simulate
                for(int i = 0; i < dashes.Count; ++i)
                {
                    if (dashes[i] == arrowDash)
                    {
                        // swap for last dash for the one we will use
                        Transform temp = dashes[dashesToUse - 1];
                        dashes[dashesToUse - 1] = arrowDash;
                        dashes[i] = temp;
                        break;
                    }
                }
            }

            float distFromLastDash = 0.0f;
            for (int i = 0; i < dashesToUse;  ++i) // place dashes
			{
                while(true) // simulation loop
                {
                    lastPos = lastPos + (lastVec * simulationDT);
                    distFromLastDash += (simulationDT * lastVec).magnitude;

                    //lastVec = projectilePhysics.WindEffect(lastVec, simulationDT);
                    lastVec = lastVec + (simulationDT * gravity);

                    if (distFromLastDash > distanceBetweenDashes)
                    {
                        distFromLastDash -= distanceBetweenDashes;

                        dashes[i].position = lastPos;
                        break;
                    }
                }

                // Activate sprite
                dashes[i].gameObject.SetActive(true);

                // Setup sprite
                var newPos = lastPos + offset;
                dashes[i].position = newPos;
                dashes[i].LookAt(newPos - Vector3.forward, lastVec);
			}


            for (int i = dashesToUse; i < dashes.Count; ++i) // disable any unused dashes
            {
                dashes[i].gameObject.SetActive(false);
            }
        }
		
		
	}

    private void UpdateDemoActions()
    {
        if (gameplayerController.targetTrans != null)
        {
            if (Input.GetKey(KeyCode.Plus) ||
               Input.GetKey(KeyCode.Equals))
            {
                ChangeHealthEvent de;
                de.delta = 0.35f;
                de.origin = transform;
                FFMessageBoard<ChangeHealthEvent>.SendToLocalToAllConnected(de, gameplayerController.targetTrans.gameObject);
            }
            if (Input.GetKey(KeyCode.Minus) ||
               Input.GetKey(KeyCode.Underscore))
            {
                ChangeHealthEvent de;
                de.delta = -0.35f;
                de.origin = transform;
                FFMessageBoard<ChangeHealthEvent>.SendToLocalToAllConnected(de, gameplayerController.targetTrans.gameObject);
            }
        }
    }

    void UpdatePlayerInput(UpdateTurn e)
	{
		playerState = PlayerState.None;

        // Update UIState
        {
            if (Input.GetKey(KeyCode.Tab))
            {
                switch (uiState)
                {
                    case UIState.Game: // -> Weapons
                        uiState = UIState.Weapons;
                        break;
                    case UIState.Weapons: // -> Game
                        uiState = UIState.Game;
                        break;
                    case UIState.Menu: // Nothing
                        break;
                }
            }
            if (Input.GetKey(KeyCode.Escape))
            {
                switch (uiState)
                {
                    case UIState.Game: // -> Menu
                        uiState = UIState.Menu;
                        break;
                    case UIState.Weapons: // -> Game
                        uiState = UIState.Game;
                        break;
                    case UIState.Menu: // -> Game
                        uiState = UIState.Game;
                        break;
                }
            }
        }

        // Don't do anything if we aren't in the UI game state
        if (uiState != UIState.Game)
            return;

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

		{ 
            // Shooting?
			if(Input.GetKey(KeyCode.Space))
            {
                firingTimer += e.dt;
                shootSpeed = Mathf.Lerp(minShootSpeed, MaxShootSpeed, Mathf.Min(firingTimer, firingTime) / firingTime);

                playerState |= PlayerState.Shooting;
            }
            else
            {
                shootSpeed = minShootSpeed;
                firingTimer = 0.0f;
            }
		}

    }



}





