using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;


// This is the controller of the splace screen object
public class SceneTransition : FFComponent {

    public float OpenFadeTime;
    public float PreFadeTime;
    public float FadeTime;
    public float PostFadeTime;

    public string LevelToLoad = "Level_01";
    
    public enum StartTransitionToNextLevel
    {
        KeyPress,
        Trigger,
    }
    private FFAction.ActionSequence FadeSequence;
    public StartTransitionToNextLevel trigger;
    public string triggerName = "";

    // Use this for initialization
    void Start () {

        // Initialize
        {
            FadeSequence = action.Sequence();
        }

        // Fade Sequence
        {
            FadeSequence.Property(
                ffSpriteColor,
                new Color(ffSpriteColor.Val.r, ffSpriteColor.Val.g, ffSpriteColor.Val.b, 0.0f),
                FFEase.E_SmoothStart,
                OpenFadeTime);
            
            FadeSequence.Sync();
            FadeSequence.Delay(PreFadeTime);
            FadeSequence.Sync();

            if(trigger == StartTransitionToNextLevel.KeyPress)
            {
                InputUpdate();
            }
            else
            {
                var triggerBox = FFMessageBoard<CustomEvent>.Box(triggerName);
                triggerBox.Connect(OnTriggerFade);
            }
        }
	}

    private void OnDestroy()
    {
        var triggerBox = FFMessageBoard<CustomEvent>.Box(triggerName);
        triggerBox.Disconnect(OnTriggerFade);
    }

    private void OnTriggerFade(CustomEvent e)
    {
        FadeToNextLevel();
    }

    // self queuing message
    void InputUpdate()
    {
        var fadeToNextLevel =
            Input.GetButtonUp("A" + 1) ||
            Input.GetButtonUp("A" + 2) ||
            Input.GetButtonUp("A" + 3) ||
            Input.anyKey;

        if (fadeToNextLevel)
        {
            FadeToNextLevel();
        }
        else // continue to check for update
        {
            FadeSequence.Sync();
            FadeSequence.Call(InputUpdate);
        }
    }

    void FadeToNextLevel()
    {
        FadeSequence.Sync();
        FadeSequence.Property(
            ffSpriteColor,
            new Color(ffSpriteColor.Val.r, ffSpriteColor.Val.g, ffSpriteColor.Val.b, 1.0f),
            FFEase.E_SmoothStartEnd,
            FadeTime);

        FadeSequence.Sync();
        FadeSequence.Delay(PostFadeTime);

        FadeSequence.Sync();
        FadeSequence.Call(LoadTransitionLevel);
    }
    
    void LoadTransitionLevel()
    {
        SceneManager.LoadScene(LevelToLoad);
    }
	
}
