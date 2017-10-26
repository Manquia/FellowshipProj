using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationsController : FFComponent {

    FFAction.ActionSequence seq;

    public FFPath StartEndPoints;
    Vector3 scaleSave;
    Color textColorSave;
    TextMesh barText;
    //SpriteRenderer barBackground;
    public string[] DemoNotificationMessages;
    public float displayTime;
    public float scaleMultiplier = 2.0f;
    float scaleMultiplierSave;

    
    // Use this for initialization
    void Start()
    {
        scaleSave = transform.localScale;
        scaleMultiplierSave = scaleMultiplier;

        seq = action.Sequence();

        FFMessage<ShowNotificationBar>.Connect(OnShowNotificationBar);
        FFMessage<HideNotificationBar>.Connect(OnHideNotificationBar);


        barText = transform.Find("Text").GetComponent<TextMesh>();
        textColorSave = barText.color;

        reset();
    }

    void OnDestroy()
    {
        FFMessage<ShowNotificationBar>.Disconnect(OnShowNotificationBar);
        FFMessage<HideNotificationBar>.Disconnect(OnHideNotificationBar);
    }


    private void OnShowNotificationBar(ShowNotificationBar e)
    {        
        var rand = Random.Range(0, DemoNotificationMessages.Length - 1);
        barText.text = DemoNotificationMessages[rand];
        barText.color = textColorSave.MakeClear();

        Activate();
    }
    private void OnHideNotificationBar(HideNotificationBar e)
    {
        Deactivate();
    }
    
    void Update()
    {
    }
    

    public float deactivationTime = 0.4f;
    public AnimationCurve deactivationMoveCurve;
    public AnimationCurve deactivationScaleCurve;
    public AnimationCurve deactivationColorCurve;
    void Deactivate()
    {
        seq.ClearSequence();

        seq.Property(ffposition, StartEndPoints.PositionAtPoint(0), deactivationMoveCurve, deactivationTime);
        seq.Property(ffscale, scaleSave * scaleMultiplier, deactivationScaleCurve, deactivationTime);
        seq.Property(barText.ffTextMeshColor(), textColorSave.MakeClear(), deactivationColorCurve, deactivationTime);

        seq.Sync();
        seq.Call(reset);
    }

    public float activationTime = 0.9f;
    public AnimationCurve activateMoveCurve;
    public AnimationCurve activateScaleCurve;
    public AnimationCurve activateColorCurve;
    void Activate()
    {
        seq.ClearSequence();

        seq.Property(ffposition, StartEndPoints.PositionAtPoint(1), activateMoveCurve, activationTime);
        seq.Property(ffscale, scaleSave * scaleMultiplier, activateScaleCurve, activationTime);
        seq.Property(barText.ffTextMeshColor(), textColorSave, activateColorCurve, activationTime);

        seq.Sync();
        seq.Call(ActiveLoop);
    }


    public float LoopTime = 1.0f;
    public float loopPositionScale = 10.0f;
    //public AnimationCurve loopPositionCurveX;
    public AnimationCurve loopPositionCurveY;
    void ActiveLoop()
    {
        seq.ClearSequence();

        //seq.Property(ffposition,
        //    StartEndPoints.PositionAtPoint(1) + (Vector3.right * loopPositionScale),
        //    loopPositionCurveX,
        //    LoopTime);

        seq.Property(ffposition,
            StartEndPoints.PositionAtPoint(1) + (Vector3.up * loopPositionScale),
            loopPositionCurveY,
            LoopTime);

        seq.Sync();
        seq.Call(ActiveLoop);
    }

    // to avoid any strange timing problems
    void reset()
    {
        transform.localScale = scaleSave;
        scaleMultiplier = scaleMultiplierSave;
        barText.color = textColorSave.MakeClear();
    }

}
