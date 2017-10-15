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
    void RemoveMenuSelectionVisuals()
    {
        ButtonHover bh;
        bh.button = null;
        bh.over = false;
        FFMessage<ButtonHover>.SendToLocal(bh);
    }
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
            PushMenuState(MenuState.MainMenu, true);
        }
    }
    #endregion

    #region Menu GOTO

    public void GOTO_BACK()
    {
        PopMenuState();
        RemoveMenuSelectionVisuals();
    }
    public void GOTO_MAINMENU()
    {
        SquashMenuState(MenuState.MainMenu);
        RemoveMenuSelectionVisuals();
        SceneManager.LoadScene("MainMenu");
        
    }
    public void GOTO_CONTROLS()
    {
        PushMenuState(MenuState.Controls);
        RemoveMenuSelectionVisuals();
    }
    public void GOTO_GAMEMENU()
    {
        SquashMenuState(MenuState.GameMenu);
        RemoveMenuSelectionVisuals();
    }
    public void GOTO_QUITDIALOG()
    {
        PushMenuState(MenuState.QuitDialog);
        RemoveMenuSelectionVisuals();
    }
    public void GOTO_RESTARTDIALOG()
    {
        PushMenuState(MenuState.RestartDialog);
        RemoveMenuSelectionVisuals();
    }

    private void GOTO_QUITTOMENUDIALOG()
    {
        PushMenuState(MenuState.QuitToMenuDialog);
        RemoveMenuSelectionVisuals();
    }

    private void GOTO_OPTIONS()
    {
        PushMenuState(MenuState.Options);
        RemoveMenuSelectionVisuals();
    }

    #endregion

    
    #region Menu ACT

    private void ACT_ResumeGame()
    {
        if (buttonActive)
        {
            RemoveMenuSelectionVisuals();
            PushMenuState(MenuState.PlayGame, true);
        }
    }
    public void ACT_Quit()
    {
        if (buttonActive)
        {
            RemoveMenuSelectionVisuals();
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
            RemoveMenuSelectionVisuals();
            MenuController.ClearMenuStates();

            QuietPushMenuState(MenuState.GameMenu, true);
            PushMenuState(MenuState.PlayGame, true);
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
            case MenuState.MainMenu:
                GOTO_MAINMENU();
                break;
            case MenuState.QuitDialog:
                GOTO_QUITDIALOG();
                break;
            case MenuState.RestartDialog:
                GOTO_RESTARTDIALOG();
                break;
            case MenuState.Controls:
                GOTO_CONTROLS();
                break;
            case MenuState.Game:
                ACT_ResumeGame();
                break;
            case MenuState.GameMenu:
                GOTO_GAMEMENU();
                break;
            case MenuState.GameControls:
                GOTO_CONTROLS();
                break;
            case MenuState.GameQuit:
                ACT_Quit();
                break;
            case MenuState.Options:
                GOTO_OPTIONS();
                break;
            case MenuState.PlayGame:
                ACT_Play();
                break;
            case MenuState.Back:
                GOTO_BACK();
                break;
            case MenuState.QuitToMenuDialog:
                GOTO_QUITTOMENUDIALOG();
                break;
            default:
                Debug.LogError("UNHANDED: Button Controller doesn't handle a PointerUp ButtonGoesTo State: " + ButtonGoesTo);
                break;
        }

        // handle Audio
        switch(ButtonGoesTo)
        {
            case MenuState.PlayGame:
                UISpeaker.Play(UISpeakerEvent.Voice.PlayGame);
                break;
            case MenuState.Back:
                UISpeaker.Play(UISpeakerEvent.Voice.ButtonBack);
                break;



            default:
                UISpeaker.Play(UISpeakerEvent.Voice.ButtonClicked);
                break;
        }
    }
    

    public override void OnPointerEnter(PointerEventData data)
    {
        ButtonHover bh;
        bh.button = GetComponent<RectTransform>();
        bh.over = true;
        FFMessage<ButtonHover>.SendToLocal(bh);

        UISpeaker.Play(UISpeakerEvent.Voice.ButtonHoverOn);
        UISpeaker.Play(UISpeakerEvent.Voice.ButtonHoverStart);
    }

    public override void OnPointerExit(PointerEventData data)
    {
        ButtonHover bh;
        bh.button = GetComponent<RectTransform>(); ;
        bh.over = false;
        FFMessage<ButtonHover>.SendToLocal(bh);

        UISpeaker.Play(UISpeakerEvent.Voice.ButtonHoverOff);
    }


#endregion

}
