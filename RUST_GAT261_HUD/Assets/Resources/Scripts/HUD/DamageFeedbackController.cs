using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFeedbackController : FFComponent {

    FFAction.ActionSequence seq;
    Vector3 scaleSave;

    // Use this for initialization
    void Start ()
    {

        scaleSave = transform.localScale;
        scaleMultiplierSav = scaleMultiplier;

        seq = action.Sequence();


        FFMessage<DamageFeedback>.Connect(OnDamageFeedback);
    }

    void OnDestroy()
    {
        FFMessage<DamageFeedback>.Disconnect(OnDamageFeedback);
    }

    private void OnDamageFeedback(DamageFeedback e)
    {
        ShowEffect();
    }





    public float moveScale = 10.0f;
    public float scaleMultiplier = 2.0f;
    float scaleMultiplierSav;

    
    public Color spriteColor;
    public float runTime = 0.4f;
    public AnimationCurve runMoveCurve;
    public AnimationCurve runScaleCurve;
    public AnimationCurve runColorCurve;
    void ShowEffect()
    {
        seq.ClearSequence();

        seq.Property(ffposition, ffposition + (Vector3.right * moveScale), runMoveCurve, runTime);
        seq.Property(ffscale, scaleSave * scaleMultiplier, runScaleCurve, runTime);

        foreach(Transform sprite in transform)
        {
            sprite.ffSpriteColor().Setter(spriteColor.MakeClear());
            seq.Property(sprite.ffSpriteColor(), spriteColor, runColorCurve, runTime);
        }

        seq.Sync();
        seq.Call(reset);
    }
    

    // to avoid any strange timing problems
    void reset()
    {
        transform.localScale = scaleSave;
        scaleMultiplier = scaleMultiplierSav;

        foreach (Transform sprite in transform)
        {
            sprite.ffSpriteColor().Setter(spriteColor.MakeClear());
        }

    }
}
