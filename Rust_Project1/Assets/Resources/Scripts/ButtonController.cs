using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : FFComponent
{

    public MenuState ActiveState;

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

        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        FFMessage<PopMenuState>.Disconnect(OnPopMenuState);
        FFMessage<PushMenuState>.Disconnect(OnPushMenuState);
    }

    private void OnPushMenuState(PushMenuState e)
    {
        if (e.newState != ActiveState) // new state is our state
        {
            Activate();
        }
    }
    private void OnPopMenuState(PopMenuState e)
    {
        if (e.popedState == ActiveState) // poped state was ours
        {
            Deactivate();
        }
    }

    public FFRef<Color> RefButtonColor()
    {
        return new FFRef<Color>(() => transform.GetComponent<UnityEngine.UI.Image>().color, (v) => { transform.GetComponent<UnityEngine.UI.Image>().color = v; });
    }
    public FFRef<Color> RefTextColor()
    {
        return new FFRef<Color>(() => transform.Find("Text").GetComponent<UnityEngine.UI.Text>().color, (v) => { transform.Find("Text").GetComponent<UnityEngine.UI.Text>().color = v; });
    }

    float fadeTime = 1.2f;
    void FadeIn()
    {
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
        FadeSeq.ClearSequence();

        var buttonColor = RefButtonColor();
        var textColor = RefTextColor();

        FadeSeq.Property(buttonColor, Color.clear, FFEase.E_SmoothStart, fadeTime * 0.4f);
        FadeSeq.Property(textColor, Color.clear, FFEase.E_SmoothStart, fadeTime * 0.4f);
        FadeSeq.Sync();
        FadeSeq.Call(ObjectActive, false);
    }
    void Deactivate()
    {
        var button = GetComponent<UnityEngine.UI.Selectable>();
        button.enabled = false;
        FadeOut();
    }
    void Activate()
    {
        var button = GetComponent<UnityEngine.UI.Selectable>();
        button.enabled = true;
        FadeIn();
    }

    void ObjectActive(object active)
    {
        gameObject.SetActive((bool)active);
    }


    #region Menu GOTO
    
    public void GOTO_BACK()
    {
        PopMenuState pms = new PopMenuState();

        Debug.Log("poped State" + pms.popedState);
        Debug.Log("New State" + pms.newState);

        FFMessage<PopMenuState>.SendToLocal(pms);
    }

    public void GOTO_MAINMENU()
    {
        MenuController.ClearMenuStates();
        PushMenuState pms = new PushMenuState(MenuState.MainMenu);
        FFMessage<PushMenuState>.SendToLocal(pms);
    }
    public void GOTO_CONTROLS()
    {
        PushMenuState pms = new PushMenuState(MenuState.Controls);
        FFMessage<PushMenuState>.SendToLocal(pms);
    }

    public void GOTO_GAMEMENU()
    {
        MenuController.ClearMenuStates();
        PushMenuState pms = new PushMenuState(MenuState.GameMenu);
        FFMessage<PushMenuState>.SendToLocal(pms);
    }

    public void GOTO_QUITDIALOG()
    {
        PushMenuState pms = new PushMenuState(MenuState.QuitDialog);
        FFMessage<PushMenuState>.SendToLocal(pms);
    }
    public void GOTO_RESTARTDIALOG()
    {
        PushMenuState pms = new PushMenuState(MenuState.RestartDialog);
        FFMessage<PushMenuState>.SendToLocal(pms);
    }

    #endregion

    #region Menu ACT

    public void ACT_QUIT()
    {
        Application.Quit();
    }
    public void ACT_Play()
    {
        MenuController.ClearMenuStates();
        //@ TODO Activate GAme!?
    }

    #endregion
}


// Boiler for Push Pop menu
/*
// Use this for initialization
void Start()
{
    FFMessage<PopMenuState>.Connect(OnPopMenuState);
    FFMessage<PushMenuState>.Connect(OnPushMenuState);
}
private void OnDestroy()
{
    FFMessage<PopMenuState>.Disconnect(OnPopMenuState);
    FFMessage<PushMenuState>.Disconnect(OnPushMenuState);
}

private void OnPushMenuState(PushMenuState e)
{
    throw new NotImplementedException();
}

private void OnPopMenuState(PopMenuState e)
{
    throw new NotImplementedException();
}

// Update is called once per frame
void Update()
{

}
*/
