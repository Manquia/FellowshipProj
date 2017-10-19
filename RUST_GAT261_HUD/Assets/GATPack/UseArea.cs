using UnityEngine;
using System.Collections;
using System;

class UseEvent
{
    public Transform player;
    public bool BlockingAction;
}
struct BlockUseArea
{
}
struct UnBlockUseArea
{
}

public class UseArea : FFComponent {


    bool useAreaBlocked = false;
    bool wantsToUse = false;

    // Use this for initialization
    void Start ()
    {
        cursorDelaySeq = action.Sequence();
        FFMessageBoard<UnBlockUseArea>.Connect(OnUnBlockUseArea, gameObject);
        FFMessageBoard<BlockUseArea>.Connect(OnBlockUseArea, gameObject);
	}
    
    private void OnUnBlockUseArea(UnBlockUseArea e)
    {
        useAreaBlocked = false;
    }
    private void OnBlockUseArea(BlockUseArea e)
    {
        useAreaBlocked = true;
    }


    // Update is called once per frame
    void Update ()
    {
        // Is Button Pressed
        if(Input.GetMouseButtonDown(0)) // left mouse button
        {
            wantsToUse = true;
        }
        else
        {
            wantsToUse = false;
        }

        

        // Player action is Busy
        if (useAreaBlocked)
        {
            if (wantsToUse)
            {
                // UseArea is blocked AND use area is trying to be used
                // @TODO Add notification, Icon, Sound here if appropriate
                ShowCustomTextMessage sctm;
                sctm.text = "You are currently busy\n right-click to release.";
                sctm.color = Color.gray;
                sctm.timeToDisplay = 2.0f;
                FFMessage<ShowCustomTextMessage>.SendToLocal(sctm);

                SetCursor(BusySprite, new Color(1, 0.5f, 0.5f, 1.0f));
            }
            else
            {
                // UseArea is blocked 
                // @TODO Add notification, Icon, Sound here if appropriate
                SetCursor(BusySprite, Color.white);
            }
            return; // do not continue
        }

    }

    
    void OnTriggerStay(Collider other)
    {
        HandleInsideTrigger(other);
    }

    public Sprite BusySprite;
    public Sprite IdleSprite;

    void HandleInsideTrigger(Collider other)
    {
        if (wantsToUse && !useAreaBlocked)
        {
            UseEvent pue = new UseEvent();
            pue.player = transform.parent; // Will be parent
            pue.BlockingAction = false; // event output

            FFMessageBoard<UseEvent>.SendToLocal(pue, other.gameObject);

            // Player action is blocking
            useAreaBlocked = pue.BlockingAction;

            // disreguard playerwant to use when we become blocked by action
            wantsToUse = !pue.BlockingAction; 
        }
        
        SetCursorDelayed(IdleSprite, Color.white, 0.15f);
    }

    void SetCursor(Sprite sprite, Color color)
    {
        ChangePlayerCursorEvent cpce;
        cpce.color = color;
        cpce.sprite = sprite;
        FFMessage<ChangePlayerCursorEvent>.SendToLocal(cpce);
    }


    FFAction.ActionSequence cursorDelaySeq;
    Sprite cursorDelaySprite;
    Color cursorDelayColor;
    void SetCursorDelayed(Sprite sprite, Color color, float delay = 0.5f)
    {
        cursorDelaySprite = sprite;
        cursorDelayColor = color;

        cursorDelaySeq.ClearSequence();
        cursorDelaySeq.Delay(delay);
        cursorDelaySeq.Sync();
        cursorDelaySeq.Call(SetCursorDelayedSeq);
    }
    void SetCursorDelayedSeq()
    {
        SetCursor(cursorDelaySprite, cursorDelayColor);
    }
}
