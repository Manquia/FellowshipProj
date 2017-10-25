using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class ButtonTransitionHandler : EventTrigger {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    MenuState ButtonGoesTo
    {
        get
        {
            return GetComponent<ButtonController>().ButtonGoesTo;
        }
    }
    bool buttonActive
    {
        get
        {
            return GetComponent<ButtonController>().buttonActive;
        }
    }

    #region helpers
    public void PopMenuState()
    {
        if (buttonActive)
        {
            PopMenuState pms = new PopMenuState();
            FFMessage<PopMenuState>.SendToLocal(pms);
        }
    }
    public void PushMenuState(MenuState state, bool forcePush = false)
    {
        if (forcePush ||
            (buttonActive && state != MenuController.GetState()))
        {
            PushMenuState pms = new PushMenuState(state);
            FFMessage<PushMenuState>.SendToLocal(pms);
        }
    }

    public void QuietPushMenuState(MenuState state, bool forcePush = false)
    {
        if (forcePush ||
            (buttonActive && state != MenuController.GetState()))
        {
            MenuController.PushStateQuiet(state);
        }
    }

    public void QuietPopMenuState(MenuState state, bool forcePush = false)
    {
        if (forcePush ||
            (buttonActive && state != MenuController.GetState()))
        {
            MenuController.PopStateQuiet();
        }
    }
    public void SquashMenuState(MenuState state)
    {
        if (buttonActive)
        {
            MenuController.ClearMenuStates();
            PushMenuState(state, true);
        }
    }
    #endregion

    #region Menu GOTO
    

    #endregion


    #region Menu ACT

    private void ACT_ResumeGame()
    {

    }


    public void ACT_Quit()
    {
        if (buttonActive)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    public void ACT_Play()
    {
        if (buttonActive)
        {
            MenuController.ClearMenuStates();

            //QuietPushMenuState(MenuState.GameMenu, true);
            //PushMenuState(MenuState.PlayGame, true);
            SceneManager.LoadScene("World");
        }
    }

    private void ACT_PlayTutorial()
    {
        if (buttonActive)
        {
            MenuController.ClearMenuStates();

            //QuietPushMenuState(MenuState.GameMenu, true);
            //PushMenuState(MenuState.PlayGame, true);
            SceneManager.LoadScene("Tutorial");
        }
    }



    private void ACT_RestartLevel()
    {
        if (buttonActive)
        {
            MenuController.ClearMenuStates();
            
            // Restart Current Level
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    #endregion

    #region Button


    public override void OnPointerUp(PointerEventData data)
    {

        // Menu Control
        switch (ButtonGoesTo)
        {
            case MenuState.None:
                break;
            default:
                Debug.LogError("UNHANDED: Button Controller doesn't handle a PointerUp ButtonGoesTo State: " + ButtonGoesTo);
                break;
        }

        // handle Audio
        switch (ButtonGoesTo)
        {


            default:
                UISpeaker.Play(UISpeakerEvent.Voice.ButtonClicked);
                break;
        }
    }

    public override void OnPointerEnter(PointerEventData data)
    {

        UISpeaker.Play(UISpeakerEvent.Voice.ButtonHoverOn);
        UISpeaker.Play(UISpeakerEvent.Voice.ButtonHoverStart);
    }

    public override void OnPointerExit(PointerEventData data)
    {

        UISpeaker.Play(UISpeakerEvent.Voice.ButtonHoverOff);
    }


    #endregion

}
