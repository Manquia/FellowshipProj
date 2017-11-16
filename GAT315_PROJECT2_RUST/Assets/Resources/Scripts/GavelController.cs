using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GavelController : FFComponent, Interactable {


    FFAction.ActionSequence audioSeq;

    public ToolTip tooltip;

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
    AudioSource CrowdNoise;

	// Use this for initialization
	void Start ()
    {
        GavelRoot = transform.Find("Gavel");
        CrowdNoise = GameObject.Find("CrowdNoise").GetComponent<AudioSource>();
        audioSeq = action.Sequence();
        crowdNoiseVolumeSave = CrowdNoise.volume;

        chosenSentence = false;
        state = State.Idle;
        tooltip.toolTipTitle = "Start Session";
        FFMessage<BeginCharacterHearing>.Connect(OnBeginCharacterHearing);
        FFMessage<SentenceChosen>.Connect(OnSentenceChosen);
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
                    tooltip.toolTipTitle = "Start Session";
                    break;
                case State.OpenSession:
                    tooltip.toolTipTitle = "Pass Judgement";
                    break;
            }
        }
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
                FadeInCrowdNoise();

                EndCharacterHearing e;
                FFMessage<EndCharacterHearing>.SendToLocal(e);

                SendOutLastCharacter sonc;
                FFMessage<SendOutLastCharacter>.SendToLocal(sonc);
                    
            }
            break;
        }
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
        audioSeq.Property(CrowdVolumeRef(), crowdNoiseVolumeSave, FFEase.E_SmoothEnd, 2.0f);
    }

    FFRef<float> CrowdVolumeRef()
    {
        return new FFRef<float>(
            () => CrowdNoise.volume,
            (v) => { CrowdNoise.volume = v; });
    }



}
