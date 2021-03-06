﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GavelController : FFComponent, Interactable {


    FFAction.ActionSequence audioSeq;

    public Transform tooltip;

    public float delayBetweenActions = 3.5f;
    float delayTimer = 0.0f;

    public enum State
    {
        None,
        Idle,
        OpenSession,
    }

    State state;
    
    bool chosenSentence = false;
    Transform GavelRoot;
    float crowdNoiseVolumeSave;
    float adjustedVolume;
    AudioSource CrowdNoise;


    CrowdManager crowdManager;
	// Use this for initialization
	void Start ()
    {
        GavelRoot = transform.Find("Gavel");
        CrowdNoise = GameObject.Find("CrowdNoise").GetComponent<AudioSource>();
        audioSeq = action.Sequence();
        crowdNoiseVolumeSave = CrowdNoise.volume;

        chosenSentence = false;
        state = State.Idle;
        FFMessage<BeginCharacterHearing>.Connect(OnBeginCharacterHearing);
        FFMessage<SentenceChosen>.Connect(OnSentenceChosen);

        CrowdVolumeRef().Setter(0.0f); // set crowd volume to zero
        crowdManager = GameObject.Find("CrowdManager").GetComponent<CrowdManager>();
        UpdateCrowd(); // use crowd manager to setup audience crowd

    }
    void OnDestroy()
    {
        FFMessage<BeginCharacterHearing>.Disconnect(OnBeginCharacterHearing);
        FFMessage<SentenceChosen>.Disconnect(OnSentenceChosen);
    }

    private void OnSentenceChosen(SentenceChosen e)
    {
        chosenSentence = true;
    }

    private void OnBeginCharacterHearing(BeginCharacterHearing e)
    {
        chosenSentence = false;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        //Debug.Log("Updaate");
        delayTimer += Time.fixedDeltaTime;

        var tooltipTitle = tooltip.Find("Background").Find("Title").GetComponent<UnityEngine.UI.Text>();
        if (delayTimer < delayBetweenActions || ReadyToPreceed() == false)
        {
            tooltip.gameObject.SetActive(false);
        }
        else
        {
            tooltip.gameObject.SetActive(true);
            switch (state)
            {
                case State.None:
                    break;
                case State.Idle:
                    tooltipTitle.text= "Start Session";
                    break;
                case State.OpenSession:
                    tooltipTitle.text = "Pass Judgement";
                    break;
            }
        }


        // tooltip faces player
        var playerCamera = GameObject.Find("Camera").transform;
        tooltip.transform.LookAt(playerCamera, Vector3.up);
        tooltip.transform.Rotate(0, 180, 0);
        var vecToCamera = tooltip.transform.position - playerCamera.position;
        var distToCamera = vecToCamera.magnitude;

        tooltip.transform.localScale = new Vector3(1 + distToCamera, 1 + distToCamera, 1 + distToCamera);
        
    }

    public void MouseOver(bool active)
    {
    }

    bool ReadyToPreceed()
    {
        if (delayTimer < delayBetweenActions)
            return false;

        switch (state)
        {
            case State.None:
                break;
            case State.Idle:
                return true;
            case State.OpenSession:
            {
                // Have we chosen a Sentence?
                if (chosenSentence == false)
                    return false;
            }
            break;
        }

        return true;
    }

    public void Use()
    {
        if (ReadyToPreceed() == false)
        {
            return;
        }
        else
        {
            // reset timer
            delayTimer = 0.0f;

            // Do animation
            var gavelAnim = GavelRoot.GetComponent<Animator>();
            gavelAnim.Play("StrikeGavel");
            var gavelSound = GavelRoot.GetComponent<AudioSource>();
            gavelSound.Play();
        }
        

        // Change to next state
        switch (state)
        {
            case State.None:
                Debug.Assert(false, "Error, in None state in GavelController");
                break;

            case State.Idle:
            {
                state = State.OpenSession;
                FadeOutCrowdNoise();

                SendInNextCharacter sinc;
                FFMessage<SendInNextCharacter>.SendToLocal(sinc);

                BeginCharacterHearing bch;
                FFMessage<BeginCharacterHearing>.SendToLocal(bch);
            }
            break;

            case State.OpenSession:
            {
                state = State.Idle;
                    
                EndCharacterHearing e;
                FFMessage<EndCharacterHearing>.SendToLocal(e);

                SendOutLastCharacter sonc;
                FFMessage<SendOutLastCharacter>.SendToLocal(sonc);


                var courtRoomController = GameObject.Find("CourtRoomController").GetComponent<CountRoomController>();
                if (courtRoomController.MoreCriminalsThisWeek() == false) // are finished for the week
                {
                    // Trigger end week in court room
                    SendInNextCharacter sinc;
                    FFMessage<SendInNextCharacter>.SendToLocal(sinc);
                }

                UpdateCrowd();
            }
            break;
        }
    }

    void UpdateCrowd()
    {
        int sizeofCrowd = crowdManager.SwapCrowd();

        float volumeScale = sizeofCrowd / crowdManager.maxCrowdSize;
        adjustedVolume = Mathf.Min(1.0f,volumeScale * crowdNoiseVolumeSave + 0.5f);

        if (sizeofCrowd > 1)
            FadeInCrowdNoise();
    }

    void FadeOutCrowdNoise()
    {
        audioSeq.ClearSequence();
        audioSeq.Delay(0.6f);
        audioSeq.Sync();
        audioSeq.Property(CrowdVolumeRef(), 0.0f, FFEase.E_SmoothStart, 1.0f);
    }
    void FadeInCrowdNoise()
    {
        audioSeq.ClearSequence();
        audioSeq.Delay(1.4f);
        audioSeq.Sync();
        audioSeq.Property(CrowdVolumeRef(), adjustedVolume, FFEase.E_SmoothEnd, 2.0f);
    }

    FFRef<float> CrowdVolumeRef()
    {
        return new FFRef<float>(
            () => CrowdNoise.volume,
            (v) => { CrowdNoise.volume = v; });
    }



}
