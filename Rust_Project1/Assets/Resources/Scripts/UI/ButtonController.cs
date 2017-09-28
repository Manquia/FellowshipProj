using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : UIBase
{
    Color ButtonColorSave;
    Color TextColorSave;
    FFAction.ActionSequence FadeSeq;
    float fadeTime = 1.0f;

    [NonSerialized]
    public bool buttonActive = false;
    // Use this for initialization


    public MenuState ButtonGoesTo;

    private void Awake()
    {
        //Save Color
        ButtonColorSave = RefButtonColor().Val;
        TextColorSave = RefTextColor().Val;
    }
    void Start()
    {
        FadeSeq = transform.GetOrAddComponent<FFAction>().Sequence();

        FFMessage<PopMenuState>.Connect(OnPopMenuState);
        FFMessage<PushMenuState>.Connect(OnPushMenuState);

        //Save Color
        ButtonColorSave = RefButtonColor().Val;
        TextColorSave = RefTextColor().Val;

        buttonActive = false;
        FadeSeq.Call(ObjectActive, false);
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
    
    void FadeIn()
    {
        FadeSeq.ClearSequence();
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
        buttonActive = false;
        FadeOut();
    }
    public override void Activate()
    {
        var button = GetComponent<UnityEngine.UI.Selectable>();
        button.enabled = true;
        buttonActive = true;
        FadeIn();
    }
}