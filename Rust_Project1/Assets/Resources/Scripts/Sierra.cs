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
        FFMessageBoard<FallingIntoPit>.Connect(OnFallingIntoPit, gameObject);
        FFMessageBoard<OnSolidGround>.Connect(OnOnSolidGround, gameObject);

        Debug.Assert(PlayerCharacter != null, "Sierra doesn't have a valid PlayerCharacter reference");

        visionState = VisionState.Idle;
        commandState = CommandState.Idle;
        
        SierraUpdateTick();
        UpdateDialogLimiter();
        UpdateSteeringValues();
    }
    void OnDestroy()
    {
        FFMessage<StayCommand>.Disconnect(OnStayCommand);
        FFMessage<FollowCommand>.Disconnect(OnFollowCommand);
        FFMessageBoard<FallingIntoPit>.Disconnect(OnFallingIntoPit, gameObject);
        FFMessageBoard<OnSolidGround>.Disconnect(OnOnSolidGround, gameObject);
    }

    float FallTimmer = 0;
    float FallTimeTillReset = 1.5f;
    private void OnOnSolidGround(OnSolidGround e)
    {
        FallTimmer = 0.0f;
    }

    private void OnFallingIntoPit(FallingIntoPit e)
    {
        //Debug.Log("FAlling into pit!");
        FallTimmer += Time.fixedDeltaTime;

        if (FallTimmer > FallTimeTillReset)
        {
            FallTimmer = 0.0f;
            ResetPlayerToLastCheckpoint rptlc;
            FFMessage<ResetPlayerToLastCheckpoint>.SendToLocal(rptlc);
        }
    }

    public float LOSLostPlayerTime = 2.25f;
    //public float FleeDistance = 2.0f;
    //public float FleeTime = 1.0f;

    public float FleeSpeed = 3.5f;
    public float WalkSpeed = 1.25f;
    public float FleeRotSpeed = 15.5f;
    public float WalkRotSpeed = 5.5f;

    int failedLOSChecks = 0;
    public float tickRate = 0.25f;
    void SierraUpdateTick()
    {
        var steering = GetComponent<Steering>();
        var playerController = PlayerCharacter.GetComponent<PlayerController>();
        RaycastHit hit;

        bool fHasVisionOfPlayer = HasVisionOfPlayer(out hit);

        if (fHasVisionOfPlayer == false)
            ++failedLOSChecks;
        else
            failedLOSChecks = 0;

        if(fHasVisionOfPlayer && playerController.state == PlayerController.State.Ghost) // Does Sierra see us and run away?
        {
            commandState = CommandState.Terrified;
            UpdateSteeringValues();


            var vecToPlayer = PlayerCharacter.position - transform.position;
            
            var normVecAwayFromPlayer = Vector3.Normalize(-vecToPlayer);
            
            var fleeTopDownMark = transform.position +
                (Vector3.up * 100.0f) +
                new Vector3(normVecAwayFromPlayer.x, 0.0f, normVecAwayFromPlayer.z) * FleeSpeed;

            string[] maskNames = { "Default" };
            RaycastHit groundRay;
            if(Physics.Raycast(fleeTopDownMark, Vector3.down, out groundRay, 200.0f, LayerMask.GetMask(maskNames))) // use raycast down
            {
                // Steer to the gound point + 0.5 in the y direction
                steering.SetupTarget(groundRay.transform, groundRay.point + (Vector3.up * verticalSteerOffset));
            }
            else // raycast failed, just try and go in the other direction
            {
                steering.SetupTarget(null, transform.position +
                    new Vector3(normVecAwayFromPlayer.x, 0, normVecAwayFromPlayer.z) * FleeSpeed);

                // DEBUG
                transform.GetOrAddComponent<FFDebugDrawLine>().Start = transform.position;
                transform.GetOrAddComponent<FFDebugDrawLine>().End = transform.position + new Vector3(normVecAwayFromPlayer.x, 0, normVecAwayFromPlayer.z) * FleeSpeed;
                transform.GetOrAddComponent<FFDebugDrawLine>().DrawColor = Color.yellow;

            }

            // Flee dialog
            SendLimitedDialogOn(CustomEventOn.LOSSeeGhost);

            lineOfSightSeq.Delay(1.0f/FleeSpeed);
            lineOfSightSeq.Sync();
            lineOfSightSeq.Call(SierraUpdateTick);

            // Early return
            return;
        }

        //Debug.Log("Sierra Sees Commands");
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
                            UpdateSteeringToFeetOfPlayer();
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
                        UpdateSteeringToFeetOfPlayer();
                        SendLimitedDialogOn(CustomEventOn.LOSPigFound);
                    }
                    else if(visionState == VisionState.Watching) // try and keep vision of player
                    {
                        if(fHasVisionOfPlayer)
                        {
                            UpdateSteeringToFeetOfPlayer();
                        }
                        else // If we lost sight, try and wonder around to find player semi-randomly in the direction they were last seen.
                        {
                            UpdateSteeringToWonderTarget();
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
                
                // We cannot do not see the player as a ghost
                commandState = CommandState.Idle;
                steering.SetupTarget(null, transform.position);
                break;
        }
        
        lineOfSightSeq.Delay(tickRate);
        lineOfSightSeq.Sync();
        lineOfSightSeq.Call(SierraUpdateTick);
    }


    private void UpdateSteeringValues()
    {
        var steering = GetComponent<Steering>();

        if(commandState == CommandState.Terrified)
        {
            steering.maxSpeed = FleeSpeed;
            steering.rotationSpeed = FleeRotSpeed;
        }
        else
        {
            steering.maxSpeed = WalkSpeed;
            steering.rotationSpeed = WalkRotSpeed;
        }
    }

    void UpdateSteeringToFeetOfPlayer()
    {
        var steering = GetComponent<Steering>();
        string[] maskNames = { "Default" };
        RaycastHit groundRayHit;
        if (Physics.Raycast(PlayerCharacter.transform.position, Vector3.down, out groundRayHit, 100.0f, LayerMask.GetMask(maskNames)))
        {
            steering.SetupTarget(groundRayHit.transform, groundRayHit.point);
        }
        else
        {
            steering.SetupTarget(null, groundRayHit.point);
        }
    }

    void UpdateSteeringToWonderTarget()
    {
        var steering = GetComponent<Steering>();
        string[] maskNames = { "Default" };
        RaycastHit groundRayHit;

        var targetPoint = steering.targetPoint;
        var vecToTarget = targetPoint - transform.position;
        var normVecToTarget = Vector3.Normalize(vecToTarget);
        
        var newTargetPoint = targetPoint.Val +                          // Use target as base location since it gets changed anyways
            (normVecToTarget * UnityEngine.Random.Range(-0.9f, 2.5f)) + // randomly move about when searching
            (Vector3.up * 50.0f);                                       // raycast down from 50 above

        if (Physics.Raycast(newTargetPoint, Vector3.down, out groundRayHit, 100.0f, LayerMask.GetMask(maskNames)))
        {
            steering.SetupTarget(groundRayHit.transform, groundRayHit.point);
        }
        else
        {
            steering.SetupTarget(null, groundRayHit.point);
        }
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
    bool HasVisionOfPlayer(out RaycastHit hit)
    {
        var vecToPlayer = PlayerCharacter.position - transform.position;
        var normVecToPlayer = Vector3.Normalize(vecToPlayer);
        var rayDistance = 25.0f;
        
        string[] layerMaskNames = { "Default", "Physical", "Ghost" };
        int raycastMask = LayerMask.GetMask(layerMaskNames); // default

        var capsuleCollider = GetComponent<CapsuleCollider>();

        var center = transform.position +
            (Vector3.up * capsuleCollider.height * transform.lossyScale.y * 0.5f * 0.5f); // offset to be up by 3/4 height

        var leftOffset = Vector3.Normalize(Vector3.Cross(normVecToPlayer, Vector3.up)) * 
            Mathf.Min(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) * capsuleCollider.radius * 0.65f;
        var rightOffset= -leftOffset;

        // Rays come from about the eyes
        leftOffset += normVecToPlayer * capsuleCollider.radius * 0.1f;
        rightOffset += normVecToPlayer * capsuleCollider.radius * 0.1f;

        // Left Ray hits
        if (Physics.Raycast(center + leftOffset, normVecToPlayer, out hit, rayDistance, raycastMask))
        {
            Debug.DrawLine(center + leftOffset, center + leftOffset + (normVecToPlayer * rayDistance), Color.red);
            if (hit.transform != PlayerCharacter) // did the ray hit the player?
            {
                //Debug.Log("Hit Thing: " + hit.transform.name);
                return false;
            }
        }
        else return false;
        // right ray hits
        if (Physics.Raycast(center + rightOffset, normVecToPlayer, out hit, rayDistance, raycastMask))
        {
            Debug.DrawLine(center + rightOffset, center + rightOffset + (normVecToPlayer * rayDistance), Color.red);
            if (hit.transform != PlayerCharacter) // did the ray hit the player?
            {
                //Debug.Log("Hit Thing: " + hit.transform.name);
                return false;
            }
        }
        else return false;
        // Center Ray hits
        if (Physics.Raycast(transform.position, normVecToPlayer, out hit, rayDistance, raycastMask))
        {
            Debug.DrawLine(center, center + (normVecToPlayer * rayDistance), Color.red);
            if (hit.transform != PlayerCharacter) // did the ray hit the player?
            {
                //Debug.Log("Hit Thing: " + hit.transform.name);
                return false;
            }
        }
        else return false;

        // No rays hit anything
        return true;
    }

    #endregion  

    private void OnFollowCommand(FollowCommand e)
    {
        var steering = GetComponent<Steering>();
        var playerController = PlayerCharacter.GetComponent<PlayerController>();

        RaycastHit hit;
        if(playerController.state == PlayerController.State.Ghost)
        {
            // Hears a ghost, no the pig!
            // @TODO Add dialogue here
            return;
        }

        commandState = CommandState.Follow;
        UpdateSteeringValues();

        if (HasVisionOfPlayer(out hit))
        {
            visionState = VisionState.Watching;
            steering.SetupTarget(PlayerCharacter.transform, e.point);
        }
        else // Failed to see vision, Event!!
        {
            visionState = VisionState.Searching;
            SendLimitedDialogOn(CustomEventOn.LOSCantFindPig);
        }
    }

    public float verticalSteerOffset = 1.0f;
    private void OnStayCommand(StayCommand e)
    {
        var steering = GetComponent<Steering>();
        var playerController = PlayerCharacter.GetComponent<PlayerController>();
        RaycastHit hit;

        if (playerController.state == PlayerController.State.Ghost)
        {
            // Hears a ghost, no the pig!
            // @TODO Add dialogue here
            return;
        }


        commandState = CommandState.Stay;
        UpdateSteeringValues();

        if (HasVisionOfPlayer(out hit))
        {
            visionState = VisionState.Idle;

            string[] maskNames = { "Default" };

            if(Physics.Raycast(e.point, Vector3.down, out hit, 100.0f, LayerMask.GetMask(maskNames) ))
            {
                steering.SetupTarget(hit.transform, e.point + (Vector3.up * verticalSteerOffset));
            }
            else // nothing to be relative
            {
                steering.SetupTarget(null, e.point);
            }
                
        }
        else // Failed to see vision, Event!!
        {
            visionState = VisionState.Searching;

            SendLimitedDialogOn(CustomEventOn.LOSCantFindPig);
        }
    }

}
