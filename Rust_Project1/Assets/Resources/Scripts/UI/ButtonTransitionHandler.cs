using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public void PushMenuState(MenuState state)
    {
        if (buttonActive && state != MenuController.GetState())
        {
            PushMenuState pms = new PushMenuState(state);
            FFMessage<PushMenuState>.SendToLocal(pms);
        }
    }
    public void SquashMenuState(MenuState state)
    {
        if (buttonActive)
        {
            MenuController.ClearMenuStates();
            PushMenuState(MenuState.MainMenu);
        }
    }
    #endregion

    #region Menu GOTO

    public void GOTO_BACK()
    {
        PopMenuState();
    }
    public void GOTO_MAINMENU()
    {
        SquashMenuState(MenuState.MainMenu);
    }
    public void GOTO_CONTROLS()
    {
        PushMenuState(MenuState.Controls);
    }
    public void GOTO_GAMEMENU()
    {
        SquashMenuState(MenuState.GameMenu);
    }
    public void GOTO_QUITDIALOG()
    {
        PushMenuState(MenuState.QuitDialog);
    }
    public void GOTO_RESTARTDIALOG()
    {
        PushMenuState(MenuState.RestartDialog);
    }
    private void GOTO_OPTIONS()
    {
        PushMenuState(MenuState.Options);
    }

    #endregion

    
    #region Menu ACT

    private void ACT_ResumeGame()
    {
        if (buttonActive)
        {
            RemoveMenuSelectionVisuals();
            MenuController.ClearMenuStates();
        }
    }
    public void ACT_Quit()
    {
        if (buttonActive)
        {
            RemoveMenuSelectionVisuals();
            Application.Quit();
        }
    }
    public void ACT_Play()
    {
        if (buttonActive)
        {
            RemoveMenuSelectionVisuals();
            MenuController.ClearMenuStates();
        }
    }

    #endregion

    #region Button


    public override void OnPointerUp(PointerEventData data)
    {
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
            default:
                Debug.LogError("UNHANDED: Button Controller doesn't handle a PointerUp ButtonGoesTo State: " + ButtonGoesTo);
                break;
        }
    }

    public override void OnPointerEnter(PointerEventData data)
    {
        ButtonHover bh;
        bh.button = GetComponent<RectTransform>();
        bh.over = true;
        FFMessage<ButtonHover>.SendToLocal(bh);
    }

    public override void OnPointerExit(PointerEventData data)
    {
        ButtonHover bh;
        bh.button = GetComponent<RectTransform>(); ;
        bh.over = false;
        FFMessage<ButtonHover>.SendToLocal(bh);
    }


    #endregion

}
