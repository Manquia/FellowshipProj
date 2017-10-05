using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sierra : FFComponent {

    public enum VisionState
    {
        Idle,
        Watching,
        Searching,
    }
    public enum CommandState
    {
        Idle,
        Stay,
        Follow,
        Terrified,
    }

    FFAction.ActionSequence dialogueLimiterSeq;
    FFAction.ActionSequence lineOfSightSeq;
    public Transform PlayerCharacter;

    VisionState visionState;
    CommandState commandState;

	// Use this for initialization
	void Start ()
    {
        lineOfSightSeq = action.Sequence();
        dialogueLimiterSeq = action.Sequence();

        FFMessage<StayCommand>.Connect(OnStayCommand);
        FFMessage<FollowCommand>.Connect(OnFollowCommand);

        Debug.Assert(PlayerCharacter != null, "Sierra doesn't have a valid PlayerCharacter reference");

        visionState = VisionState.Idle;
        commandState = CommandState.Idle;

        var steering = GetComponent<Steering>();
        steering.TargetTrans = PlayerCharacter;

        SierraUpdateTick();
        UpdateDialogLimiter();
    }
    void OnDestroy()
    {
        FFMessage<StayCommand>.Disconnect(OnStayCommand);
        FFMessage<FollowCommand>.Disconnect(OnFollowCommand);
    }

    public float LOSLostPlayerTime = 2.25f;
    public float FleeDistance = 2.0f;
    public float FleeTime = 1.0f;

    int failedLOSChecks = 0;
    float tickRate = 0.45f;
    void SierraUpdateTick()
    {
        var steering = GetComponent<Steering>();
        var playerController = PlayerCharacter.GetComponent<PlayerController>();

        bool fHasVisionOfPlayer = HasVisionOfPlayer();

        if (fHasVisionOfPlayer == false)
            ++failedLOSChecks;
        else
            failedLOSChecks = 0;

        if(fHasVisionOfPlayer && playerController.state == PlayerController.State.Ghost) // Does Sierra see us and run away?
        {
            var vecToPlayer = PlayerCharacter.position - transform.position;
            var normVecAwayFromPlayer = Vector3.Normalize(-vecToPlayer);

            var fleeTopDownMark = transform.position +
                new Vector3(normVecAwayFromPlayer.x, 100.0f, normVecAwayFromPlayer.z) * FleeDistance;

            RaycastHit hit;
            if(Physics.Raycast(fleeTopDownMark, Vector3.down, out hit)) // use raycast down
            {
                // Steer to the gound point + 0.5 in the y direction
                steering.targetPoint = hit.point + (Vector3.up * 0.5f);
            }
            else // raycast failed, just try and go in the other direction
            {
                steering.targetPoint = transform.position +
                    new Vector3(normVecAwayFromPlayer.x, 0, normVecAwayFromPlayer.z) * FleeDistance;
            }

            // Flee dialog
            SendLimitedDialogOn(CustomEventOn.LOSSeeGhost);

            lineOfSightSeq.Delay(FleeTime);
            lineOfSightSeq.Sync();
            lineOfSightSeq.Call(SierraUpdateTick);

            // Early return
            return;
        }


        // Sierra isn't terrified. Listen to commands
        switch (commandState)
        {
            case CommandState.Idle:
                break;
            case CommandState.Stay:
                {
                    if(visionState == VisionState.Searching)
                    {
                        if (fHasVisionOfPlayer) // given stay command, but player wasn't in LOS
                        {
                            visionState = VisionState.Idle;
                            steering.UpdateTargetPointFromTargetTrans();
                            SendLimitedDialogOn(CustomEventOn.LOSPigFound);
                        }
                        else
                        {
                            SendLimitedDialogOn(CustomEventOn.LOSCantFindPig);
                        }
                    }
                }
                break;
            case CommandState.Follow:
                {
                    if(visionState == VisionState.Searching && fHasVisionOfPlayer) // given command, looking to find player again
                    {
                        visionState = VisionState.Watching;
                        steering.UpdateTargetPointFromTargetTrans();
                        SendLimitedDialogOn(CustomEventOn.LOSPigFound);
                    }
                    else if(visionState == VisionState.Watching) // try and keep vision of player
                    {
                        if(fHasVisionOfPlayer)
                        {
                            steering.UpdateTargetPointFromTargetTrans();
                        }
                        // should sierra be worried about where the pig went?
                        if (failedLOSChecks * tickRate > LOSLostPlayerTime) // time till we say we are worried
                        {
                            visionState = VisionState.Searching;
                            SendLimitedDialogOn(CustomEventOn.LOSPigLost);
                        }
                    }
                }
                break;
            case CommandState.Terrified:
                break;
        }
        
        lineOfSightSeq.Delay(tickRate);
        lineOfSightSeq.Sync();
        lineOfSightSeq.Call(SierraUpdateTick);
    }

    #region helpers

    float dialogLimitationTime = 4.25f; 
    bool fDialogLimited = false;
    void UpdateDialogLimiter()
    {
        fDialogLimited = false;

        dialogueLimiterSeq.Delay(dialogLimitationTime);
        dialogueLimiterSeq.Sync();
        dialogueLimiterSeq.Call(UpdateDialogLimiter);
    }

    void SendLimitedDialogOn(string eventName)
    {
        if (fDialogLimited)
            return;

        fDialogLimited = true;

        CustomEventOn ceo;
        ceo.tag = eventName;
        var box = FFMessageBoard<CustomEventOn>.Box(eventName);
        box.SendToLocal(ceo);
    }
    void SendLimitedDialogOff(string eventName)
    {
        if (fDialogLimited)
            return;

        fDialogLimited = true;

        CustomEventOff ceo;
        ceo.tag = eventName;
        var box = FFMessageBoard<CustomEventOff>.Box(eventName);
        box.SendToLocal(ceo);
    }
    bool HasVisionOfPlayer()
    {
        var vecToPlayer = PlayerCharacter.position - transform.position;
        var normVecToPlayer = Vector3.Normalize(vecToPlayer);
        var rayDistance = 25.0f;
        
        RaycastHit hit;
        string[] layerMaskNames = { "Default", "Physical", "Ghost" };
        int raycastMask = LayerMask.GetMask(layerMaskNames); // default

        if(Physics.Raycast(transform.position, normVecToPlayer, out hit, rayDistance, raycastMask))
        {
            if(hit.transform == PlayerCharacter) // did the ray hit the player?
            {
                return true;
            }
        }

        return false;
    }

    #endregion  

    private void OnFollowCommand(FollowCommand e)
    {
        var steering = GetComponent<Steering>();
        commandState = CommandState.Follow;
    
        if(HasVisionOfPlayer())
        {
            visionState = VisionState.Watching;
            steering.targetPoint = e.point;
            steering.TargetTrans = e.trans;
        }
        else // Failed to see vision, Event!!
        {
            visionState = VisionState.Searching;


            SendLimitedDialogOn(CustomEventOn.LOSCantFindPig);
        }
    }

    private void OnStayCommand(StayCommand e)
    {
        var steering = GetComponent<Steering>();


        commandState = CommandState.Stay;

        if (HasVisionOfPlayer())
        {
            visionState = VisionState.Idle;
            steering.targetPoint = e.point;
            steering.TargetTrans = null;
        }
        else // Failed to see vision, Event!!
        {
            visionState = VisionState.Searching;

            SendLimitedDialogOn(CustomEventOn.LOSCantFindPig);
        }
    }

}
