using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : UIBase
{
    Color ButtonColorSave;
    Color TextColorSave;
    FFAction.ActionSequence FadeSeq;
    // Use this for initialization


    void Start()
    {
        FadeSeq = action.Sequence();

        FFMessage<PopMenuState>.Connect(OnPopMenuState);
        FFMessage<PushMenuState>.Connect(OnPushMenuState);

        //Save Color
        ButtonColorSave = RefButtonColor().Val;
        TextColorSave = RefTextColor().Val;

        buttonActive = false;
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        FFMessage<PopMenuState>.Disconnect(OnPopMenuState);
        FFMessage<PushMenuState>.Disconnect(OnPushMenuState);
    }
    
    public FFRef<Color> RefButtonColor()
    {
        return new FFRef<Color>(() => transform.GetComponent<UnityEngine.UI.Image>().color, (v) => { transform.GetComponent<UnityEngine.UI.Image>().color = v; });
    }
    public FFRef<Color> RefTextColor()
    {
        return new FFRef<Color>(() => transform.Find("Text").GetComponent<UnityEngine.UI.Text>().color, (v) => { transform.Find("Text").GetComponent<UnityEngine.UI.Text>().color = v; });
    }

    float fadeTime = 1.0f;
    bool buttonActive = false;
    void FadeIn()
    {
        FadeSeq.ClearSequence();
        buttonActive = true;
        var buttonColor = RefButtonColor();
        var textColor = RefTextColor();

        buttonColor.Setter(Color.clear);
        textColor.Setter(Color.clear);

        ObjectActive(true);
        FadeSeq.Property(buttonColor, ButtonColorSave, FFEase.E_SmoothEnd, fadeTime);
        FadeSeq.Property(textColor, TextColorSave, FFEase.E_SmoothEnd, fadeTime);
        FadeSeq.Sync();
    }
    void FadeOut()
    {
        buttonActive = false;
        FadeSeq.ClearSequence();

        var buttonColor = RefButtonColor();
        var textColor = RefTextColor();
        
        buttonColor.Setter(ButtonColorSave);
        textColor.Setter(TextColorSave);

        FadeSeq.Property(buttonColor, Color.clear, FFEase.E_SmoothStart, fadeTime * 0.12f);
        FadeSeq.Property(textColor, Color.clear, FFEase.E_SmoothStart, fadeTime * 0.12f);
        FadeSeq.Sync();
        FadeSeq.Call(ObjectActive, false);
    }

    public override void Deactivate()
    {
        var button = GetComponent<UnityEngine.UI.Selectable>();
        button.enabled = false;
        FadeOut();
    }
    public override void Activate()
    {
        var button = GetComponent<UnityEngine.UI.Selectable>();
        button.enabled = true;
        FadeIn();
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

    #endregion

    #region Menu ACT

    public void ACT_QUIT()
    {
        if(buttonActive)
            Application.Quit();
    }
    public void ACT_Play()
    {
        if (buttonActive)
            MenuController.ClearMenuStates();
    }

    #endregion
}