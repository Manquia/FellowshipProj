using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerContoller : FFComponent {

    FFAction.ActionSequence seq;

    public FFPath StartEndPoints;

    Vector3 scaleSave;

    TextMesh timeText;
    SpriteRenderer timeBackground;
    public AudioClip finishedTimerSound;
    public float DemoStartTime = 10.0f;
    float timer = 0;
    bool running = false;

	// Use this for initialization
	void Start ()
    {
        scaleSave = transform.localScale;
        scaleMultiplierSav = scaleMultiplier;

        seq = action.Sequence();

        FFMessage<ShowGameTimer>.Connect(OnShowGameTimer);
        FFMessage<HideGameTimer>.Connect(OnHideGameTimer);


        timeText = transform.Find("Text").GetComponent<TextMesh>();
        timeBackground = transform.Find("Background").GetComponent<SpriteRenderer>();
    }

    void OnDestroy()
    {
        FFMessage<ShowGameTimer>.Disconnect(OnShowGameTimer);
        FFMessage<HideGameTimer>.Disconnect(OnHideGameTimer);
    }


    private void OnShowGameTimer(ShowGameTimer e)
    {

        var audioSrc = GetComponent<AudioSource>();
        audioSrc.loop = true;
        audioSrc.Play();

        running = true;
        timer = DemoStartTime;
        Activate();
    }
    private void OnHideGameTimer(HideGameTimer e)
    {
        var audioSrc = GetComponent<AudioSource>();
        audioSrc.Stop();

        running = false;
        Deactivate();
    }

    public Color normalColor;
    public Color warnColor;
    public Color endColor;

    void Update()
    {
        if (running == false)
            return;
        

        // Update Timer
        timer = Mathf.Max(0.0f, timer - Time.deltaTime);
        

        { // Update Text contents
            string str = "";
            str += timer.ToString("0.0"); ;
            timeText.text = str;
        }

        { //  Manage Color Changes + Pulse value
            if (timer >= 5.0f)
            {
                timeBackground.color = normalColor;
            }
            else if (timer < 5.0f && timer > 0.0f) // make color change
            {
                var delta = (1 - timer / 5.2f) * 2.0f;
                scaleMultiplier = scaleMultiplierSav + (scaleMultiplierSav * delta);

                timeBackground.color = warnColor;
            }
            else
            {
                var audioSrc = GetComponent<AudioSource>();
                audioSrc.Stop();
                audioSrc.loop = false;
                audioSrc.PlayOneShot(finishedTimerSound);

                transform.localScale = scaleSave;
                seq.ClearSequence();
                timeBackground.color = endColor;
                running = false;
            }
        }
    }



    public float scaleMultiplier = 2.0f;
    float scaleMultiplierSav;

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
        scaleMultiplier = scaleMultiplierSav;
    }

    
    
}
