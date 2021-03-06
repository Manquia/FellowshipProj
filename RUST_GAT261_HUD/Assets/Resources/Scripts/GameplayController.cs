﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ShowGameTimer { } // T
public struct HideGameTimer { } // Shift + T
public struct ShowNotificationBar { } // N
public struct HideNotificationBar { } // Shift + N
public struct ShowWeatherBar { } // W
public struct HideWeatherBar { } // Shift + W
public struct DamageFeedback { } // D
public class ItemPickupNotification { public bool handled; } // P

// ChangeTarget             -> Tab
// Switch Character         -> S
// IncreaseDecreaseHealth   -> +-
// ChangeWindSpeed          -> E, Shift + E
// Aim                      -> Up, Down Arrow


public class GameplayController : FFComponent
{
    public Transform cameraTrans;
    public Vector3 cameraOffset;

    FFAction.ActionSequence updateSeq;
    FFAction.ActionSequence cameraSeq;

    public Transform TargetRedical;
    public Transform PlayerIcon;

    public struct TurnInfo
    {
        public Character character;
    }

	// Use this for initialization
	void Start ()
    {
        updateSeq = action.Sequence();
        cameraSeq = action.Sequence();

        updateSeq.Delay(Time.fixedDeltaTime * 1.5f); // wait 1-2 frames before staring game
        updateSeq.Sync();
        updateSeq.Call(BeginGame);
	}

    void Update()
    {
        UpdateDemoActions();
    }

    public AudioClip timerSound;
    public AudioClip notificationSound;
    public AudioClip windSound;
    public AudioClip damageSound;
    public AudioClip pickupItemSound;

    public AudioClip switchTargetSound;
    public AudioClip switchPlayerSound;

    void UpdateDemoActions()
    {
        // Timer
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T)) // hide timer
        {
            FFMessage<HideGameTimer>.SendToLocal(new HideGameTimer());
            AudioHandler.Play(timerSound);
        }
        else if (Input.GetKeyDown(KeyCode.T)) //show timer
        {
            //Debug.Log("Show timer");
            FFMessage<ShowGameTimer>.SendToLocal(new ShowGameTimer());
            AudioHandler.Play(timerSound);
        }

        // Notifications
        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.N))
        {
            FFMessage<HideNotificationBar>.SendToLocal(new HideNotificationBar());
            AudioHandler.Play(notificationSound);
        }
        else if (Input.GetKeyDown(KeyCode.N)) //show notification
        {
            FFMessage<ShowNotificationBar>.SendToLocal(new ShowNotificationBar());
            AudioHandler.Play(notificationSound);
        }

        // Weather
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.W))
        {
            FFMessage<HideWeatherBar>.SendToLocal(new HideWeatherBar());
            AudioHandler.Play(windSound);
        }
        else if (Input.GetKeyDown(KeyCode.W)) //show weather
        {
            FFMessage<ShowWeatherBar>.SendToLocal(new ShowWeatherBar());
            AudioHandler.Play(windSound);
        }

        // Damage
        if (Input.GetKeyDown(KeyCode.D))
        {
            FFMessage<DamageFeedback>.SendToLocal(new DamageFeedback());
            AudioHandler.Play(damageSound);
        }
		
        // Pickup Item
        if (Input.GetKeyDown(KeyCode.P))
        {
			var ipn = new ItemPickupNotification();
			ipn.handled = false;
            FFMessage<ItemPickupNotification>.SendToLocal(ipn);
            AudioHandler.Play(pickupItemSound);
        }
    }

    public struct TeamInfo
    {
        public int teamId;
        public int characterIndex;
    }
    
    List<TeamInfo> teamInfo = new List<TeamInfo>();

    int teamIndex_D = 0; // Current team active
    int teamIndex
    {
        get { return teamIndex_D % teamInfo.Count; }
        set { teamIndex_D = value;  }
    }

    int targetTeamIndex_D = 0;
    int targetTeamIndex
    {
        get { return targetTeamIndex_D % teamInfo.Count; }
        set { targetTeamIndex_D = value; }
    }
    int targetCharacterIndex = 0;
    Character targetCharacter;

    public Transform targetTrans;

    List<FFRef<Vector3>> cameraTarget = new List<FFRef<Vector3>>();

    #region StateFunctions

    // Start of the game
    // -> InitTurn
    void BeginGame() 
    {
        DisplayDebugState("BeginGame");
        // Get team Ids, Randomize order
        {
            foreach(var idTeamPair in Character.TeamRosters)
            {
                TeamInfo ti;
                ti.teamId = idTeamPair.Key;
                ti.characterIndex = 0;
                teamInfo.Add(ti);
            }

            //var teamCount = teamInfo.Count;
            //var randomSwapCount = teamCount * 10;
            //for (int i = 0; i < randomSwapCount; ++i)
            //{
            //    var randStart = UnityEngine.Random.Range(0, teamCount);
            //    var randEnd = UnityEngine.Random.Range(0, teamCount);
            //
            //    TeamInfo temp = teamInfo[randStart];
            //    teamInfo[randStart] = teamInfo[randEnd];
            //    teamInfo[randEnd] = temp;
            //}
        }

        // Start turn of first team
        updateSeq.Call(InitTurn);
    }

    // -> BeginTurn
    void InitTurn()
    {
        DisplayDebugState("InitTurn");
        // Get Info
        var ti = teamInfo[teamIndex];
        var roster = Character.TeamRosters[ti.teamId];
        var character = roster[ti.characterIndex % roster.Count];
        var characterTrans = character.transform;

        targetCharacter = character;        // target is self
        targetTrans = character.transform;  // target is self

        targetTeamIndex = teamIndex + 1;
        targetCharacterIndex = 0;

        cameraTarget.Clear(); // no camera targets

        // Add our position
        cameraTarget.Add(new FFRef<Vector3>(
            () => characterTrans.position,
            (v) => { characterTrans.position = v; }
            ));

        BeginTurn bt;
        FFMessageBoard<BeginTurn>.SendToLocal(bt, character.gameObject);

        updateSeq.Call(BeginTurn);
    }

    // -> BeginTurn
    // -> UpdateTurn
    void BeginTurn()
    {
        DisplayDebugState("BeginTurn");
        // Get Info
        var ti = teamInfo[teamIndex];
        var roster = Character.TeamRosters[ti.teamId];
        var character = roster[ti.characterIndex % roster.Count];
        
        // Move camera to active character
        FocusCameraOn(character.transform.position);

        // Wait for input before UpdateTurn starts
        if (Input.anyKey)
        {
            updateSeq.Sync();
            updateSeq.Call(UpdateTurn);
            return;
        }
        
        updateSeq.Sync();
        updateSeq.Call(BeginTurn);
    }

    // -> UpdateTurn
    // -> EndTurn
    void UpdateTurn()
    {
        DisplayDebugState("UpdateTurn");
        

        // Get Info
        var ti = teamInfo[teamIndex];
        var roster = Character.TeamRosters[ti.teamId];
        var character = roster[ti.characterIndex % roster.Count];
        var playerController = character.GetComponent<PlayerController>();


        //Debug.Log("Team Index: " + teamIndex);
        //Debug.Log("Character Index: " + ti.characterIndex);

        // Update Character
        UpdateTurn up; up.dt = Time.fixedDeltaTime;
        FFMessageBoard<UpdateTurn>.SendToLocal(up, character.gameObject);

        // Z-target an enemy
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            int shiftDirection = 1;
            if (Input.GetKey(KeyCode.LeftShift)) shiftDirection = -1;
            

            if (cameraTarget.Count > 1) // 2 targets == Already targeting enemey.
            {
                // Remove old FFRef<Vector3>
                cameraTarget.RemoveAt(1);
            }

            TeamInfo targetTeamInfo = teamInfo[targetTeamIndex];
            var targetTeamRoster = Character.TeamRosters[targetTeamInfo.teamId];

            // Apply changes to indexes, info as needed
            if (targetTeamRoster.Count <= targetCharacterIndex + shiftDirection)// finished with targetTeam, goto next
            {
                targetTeamIndex = targetTeamIndex + shiftDirection; // apply shift to target team
                targetTeamInfo = teamInfo[targetTeamIndex]; // get info of new team we are targeting
                targetTeamRoster = Character.TeamRosters[targetTeamInfo.teamId];
                targetCharacterIndex = 0;
            }
            else if (targetCharacterIndex + shiftDirection < 0)                // finished with targetTeam, goto prev)
            {
                targetTeamIndex = targetTeamIndex + shiftDirection; // apply shift to target team
                if (targetTeamIndex < 0) targetTeamIndex = teamInfo.Count - 1;

                targetTeamInfo = teamInfo[targetTeamIndex]; // get info of new team we are targeting
                targetTeamRoster = Character.TeamRosters[targetTeamInfo.teamId];
                targetCharacterIndex = targetTeamRoster.Count - 1;
            }
            else // still within same team
            {
                targetCharacterIndex += shiftDirection;
            }


            //Debug.Log("Target Team Index: " + targetTeamInfo.teamId);
            //Debug.Log("Target Index: " + targetCharacterIndex);

            targetCharacter = targetTeamRoster[targetCharacterIndex];
            targetTrans = targetCharacter.transform;

            // Add new FFRef<Vector3>
            cameraTarget.Add(new FFRef<Vector3>(
                () => targetTrans.position,
                (v) => { targetTrans.position = v; }));

            ResetCameraSeq();
            AudioHandler.Play(switchTargetSound);
        }

        // Skip to next character
        if(Input.GetKeyDown(KeyCode.S)) // skip turn
        {
            AudioHandler.Play(switchPlayerSound);
            updateSeq.Call(EndTurn);
            return;
        }

        // Update Targeting
        if(targetTrans != null)
        {
            // if target is not player, Do Target Redical
            if(targetTrans != character.transform)
                playerController.targetRedical.gameObject.SetActive(true);
            else // targeting player, don't need to show targeting redical
                playerController.targetRedical.gameObject.SetActive(false);


            // Update Targeting Redical Positions
            {
                playerController.targetRedical.position = targetTrans.position +
                    (Vector3.up * 0.3f) + (-Vector3.forward * 0.1f);
            }
        }


        updateSeq.Sync();
        updateSeq.Call(UpdateTurn);
    }
    
    // -> InitTurn
    void EndTurn()
    {
        DisplayDebugState("EndTurn");
        // Get Info
        var ti = teamInfo[teamIndex];
        var roster = Character.TeamRosters[ti.teamId];
        var character = roster[ti.characterIndex % roster.Count];

        // Set next character to be updated next frame
        ti.characterIndex += 1;
        teamInfo[teamIndex] = ti;

        EndTurn et;
        FFMessageBoard<EndTurn>.SendToLocal(et, character.gameObject);

        // @TODO, Check for Game ended?

        // Goto next team to update
        ++teamIndex;
        updateSeq.Sync();
        updateSeq.Call(InitTurn);
    }

    #endregion


    #region Helpers
    private void ResetCameraSeq()
    {
        cameraSeq.ClearSequence();
        
        Vector3 averagePos = Vector3.zero;

        foreach(var posRef in cameraTarget)
        {
            averagePos += posRef;
        }
        averagePos /= cameraTarget.Count;


        cameraSeq.Property(cameraTrans.ffposition(), averagePos + cameraOffset, FFEase.E_Continuous, 0.35f);
        cameraSeq.Sync();
        cameraSeq.Call(ResetCameraSeq);
    }

    void FocusCameraOn(Transform trans)
    {
        cameraTrans.position = trans.position + (-Vector3.forward * 10.0f);
    }
    void FocusCameraOn(Vector3 pos)
    {
        cameraTrans.position = pos + (-Vector3.forward * 10.0f);
    }

    #endregion


    void DisplayDebugState(string stateName)
    {
        var spriteText = GetComponent<TextMesh>();
        spriteText.text = stateName;
    }
}
