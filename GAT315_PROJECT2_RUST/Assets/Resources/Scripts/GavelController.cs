using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GavelController : MonoBehaviour, Interactable {

    public ToolTip tooltip;

    float delayBetweenActions = 3.5f;
    float delayTimer = 0.0f;

    public enum State
    {
        None,
        Idle,
        OpenSession,
    }

    State state;

    bool chosenSentence = false;

	// Use this for initialization
	void Start ()
    {
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
        }

        // Change to next state
        switch (state)
        {
            case State.None:
                Debug.Assert(false, "Error, in None state in GavelController");
                break;

            case State.Idle:
            {
                SendInNextCharacter sinc;
                FFMessage<SendInNextCharacter>.SendToLocal(sinc);

                BeginCharacterHearing bch;
                FFMessage<BeginCharacterHearing>.SendToLocal(bch);
                state = State.OpenSession;
            }
            break;

            case State.OpenSession:
            {
                EndCharacterHearing e;
                FFMessage<EndCharacterHearing>.SendToLocal(e);
                state = State.Idle;
            }
            break;
        }
    }

}
