using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct ShowGameTimer { }
struct HideGameTimer { }




public class GameplayController : FFComponent
{
    public Transform cameraTrans;
    public Vector3 cameraOffset;

    FFAction.ActionSequence updateSeq;
    FFAction.ActionSequence cameraSeq;


    public struct TurnInfo
    {
        public Character character;
    }

	// Use this for initialization
	void Start ()
    {
        updateSeq = action.Sequence();
        cameraSeq = action.Sequence();

        updateSeq.Call(BeginGame);
	}

    void Update()
    {
        UpdateDemoActions();
    }

    void UpdateDemoActions()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T)) // hide timer
        {
            FFMessage<HideGameTimer>.SendToLocal(new HideGameTimer());
        }
        else if (Input.GetKeyDown(KeyCode.T)) //show timer
        {
            Debug.Log("Show timer");
            FFMessage<ShowGameTimer>.SendToLocal(new ShowGameTimer());
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
    Transform targetTrans;

    List<FFRef<Vector3>> cameraTarget = new List<FFRef<Vector3>>();

    #region StateFunctions

    // Start of the game
    void BeginGame() // -> BeginTurn
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

            var teamCount = teamInfo.Count;
            var randomSwapCount = teamCount * 10;
            for (int i = 0; i < randomSwapCount; ++i)
            {
                var randStart = UnityEngine.Random.Range(0, teamCount);
                var randEnd = UnityEngine.Random.Range(0, teamCount);

                TeamInfo temp = teamInfo[randStart];
                teamInfo[randStart] = teamInfo[randEnd];
                teamInfo[randEnd] = temp;
            }
        }

        // Start turn of first team
        updateSeq.Call(BeginTurn);
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

        cameraTarget.Clear(); // no camera targets

        // Add our position
        cameraTarget.Add(new FFRef<Vector3>(
            () => characterTrans.position,
            (v) => { characterTrans.position = v; }
            ));

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

        // Update Character
        UpdatePlayer up; up.dt = Time.fixedDeltaTime;
        FFMessageBoard<UpdatePlayer>.SendToLocal(up, character.gameObject);

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

            var targetTeamInfo = teamInfo[targetTeamIndex];
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

            targetCharacter = targetTeamRoster[targetCharacterIndex];
            targetTrans = targetCharacter.transform;

            // Add new FFRef<Vector3>
            cameraTarget.Add(new FFRef<Vector3>(
                () => targetTrans.position,
                (v) => { targetTrans.position = v; }));

            ResetCameraSeq();
        }

        if(Input.GetKeyDown(KeyCode.S)) // skip turn
        {
            updateSeq.Call(EndTurn);
            return;
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

        // @TODO, Game ended?

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
