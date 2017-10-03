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

        UpdateLineOfSight();
        UpdateDialogLimiter();
    }
    void OnDestroy()
    {
        FFMessage<StayCommand>.Disconnect(OnStayCommand);
        FFMessage<FollowCommand>.Disconnect(OnFollowCommand);
    }

    public float LOSLostPlayerTime = 2.25f;
    

    int failedLOSChecks = 0;
    float tickRate = 0.45f;
    void UpdateLineOfSight()
    {
        var steering = GetComponent<Steering>();

        bool fHasVisionOfPlayer = HasVisionOfPlayer();

        if (fHasVisionOfPlayer == false)
            ++failedLOSChecks;
        else
            failedLOSChecks = 0;



        switch (commandState)
        {
            case CommandState.Idle:
                break;
            case CommandState.Stay:
                {
                    if(visionState == VisionState.Searching && fHasVisionOfPlayer) // given stay command, but player wasn't in LOS
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
        lineOfSightSeq.Call(UpdateLineOfSight);
    }

    #region helpers

    float dialogLimitationTime = 1.25f; 
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

        Ray rayToPlayer = new Ray(transform.position, vecToPlayer);
        RaycastHit hit;

        if(Physics.Raycast(transform.position, normVecToPlayer, out hit, rayDistance))
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
