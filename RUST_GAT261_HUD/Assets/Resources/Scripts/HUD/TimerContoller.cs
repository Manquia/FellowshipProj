using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerContoller : FFComponent {

    FFAction.ActionSequence seq;

    public FFPath StartEndPoints;

    Vector3 scaleSave;

	// Use this for initialization
	void Start ()
    {
        scaleSave = transform.localScale;

        seq = action.Sequence();

        FFMessage<ShowGameTimer>.Connect(OnShowGameTimer);
        FFMessage<HideGameTimer>.Connect(OnHideGameTimer);


    }

    void OnDestroy()
    {
        FFMessage<ShowGameTimer>.Disconnect(OnShowGameTimer);
        FFMessage<HideGameTimer>.Disconnect(OnHideGameTimer);
    }


    private void OnShowGameTimer(ShowGameTimer e)
    {
        Activate();
    }
    private void OnHideGameTimer(HideGameTimer e)
    {
        Deactivate();
    }



    public float scaleMultiplier = 2.0f;

    public float deactivationTime = 0.4f;
    public AnimationCurve deactivationMoveCurve;
    public AnimationCurve deactivationScaleCurve;
    void Deactivate()
    {
        seq.ClearSequence();

        seq.Property(ffposition, StartEndPoints.PositionAtPoint(0), deactivationMoveCurve, deactivationTime);
        seq.Property(ffscale, scaleSave * scaleMultiplier, deactivationScaleCurve, deactivationTime);

        seq.Sync();
        seq.Call(reset);
    }

    public float activationTime = 0.9f;
    public AnimationCurve activateMoveCurve;
    public AnimationCurve activateScaleCurve;
    void Activate()
    {
        seq.ClearSequence();

        seq.Property(ffposition, StartEndPoints.PositionAtPoint(1), activateMoveCurve, activationTime);
        seq.Property(ffscale, scaleSave * scaleMultiplier, activateScaleCurve, activationTime);

        seq.Sync();
        seq.Call(ActiveLoop);
    }


    public float LoopTime = 1.0f;
    public AnimationCurve loopScaleCurve;
    void ActiveLoop()
    {
        seq.ClearSequence();

        seq.Property(ffscale, scaleSave * scaleMultiplier, loopScaleCurve, LoopTime);

        seq.Sync();
        seq.Call(ActiveLoop);
    }

    // to avoid any strange timing problems
    void reset()
    {
        transform.localScale = scaleSave;
    }

    
    
}
