using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderController : UIBase {

    Color BackgroundColorSave;
    Color FillColorSave;
    Color HandleColorSave;

    UnityEngine.UI.Image background;
    UnityEngine.UI.Image fill;
    UnityEngine.UI.Image handle;

    FFAction.ActionSequence FadeSeq;

    private void Awake()
    {
        // Get objects
        background = transform.Find("Background").GetComponent<UnityEngine.UI.Image>();
        fill = transform.Find("Fill Area").Find("Fill").GetComponent<UnityEngine.UI.Image>();
        handle = transform.Find("Handle Slide Area").Find("Handle").GetComponent<UnityEngine.UI.Image>();
        
        // Save color
        BackgroundColorSave = background.color;
        FillColorSave       = fill.color;
        HandleColorSave     = handle.color;
    }
    // Use this for initialization
    void Start()
    {
        FadeSeq = transform.GetOrAddComponent<FFAction>().Sequence();
        FFMessage<PopMenuState>.Connect(OnPopMenuState);
        FFMessage<PushMenuState>.Connect(OnPushMenuState);

        FadeSeq.Call(ObjectActive, false);
    }
    private void OnDestroy()
    {
        FFMessage<PopMenuState>.Disconnect(OnPopMenuState);
        FFMessage<PushMenuState>.Disconnect(OnPushMenuState);
    }

    public override void Deactivate()
    {
        FadeOut();
    }
    public override void Activate()
    {
        FadeIn();
    }



    public FFRef<Color> RefBackgroundColor()
    {
        return new FFRef<Color>(() => background.color, (v) => { background.color = v; });
    }
    public FFRef<Color> RefFillColor()
    {
        return new FFRef<Color>(() => fill.color, (v) => { fill.color = v; });
    }
    public FFRef<Color> RefHandleColor()
    {
        return new FFRef<Color>(() => handle.color, (v) => { handle.color = v; });
    }
    float fadeTime = 0.9f;

    void FadeIn()
    {
        FadeSeq.ClearSequence();

        var backgroundColor = RefBackgroundColor();
        var fillColor = RefFillColor();
        var handleColor = RefHandleColor();

        backgroundColor.Setter(BackgroundColorSave.MakeClear());
        fillColor.Setter(FillColorSave.MakeClear());
        handleColor.Setter(HandleColorSave.MakeClear());

        ObjectActive(true);
        FadeSeq.Property(backgroundColor, BackgroundColorSave, FFEase.E_SmoothEnd, fadeTime);
        FadeSeq.Property(fillColor, FillColorSave, FFEase.E_SmoothEnd, fadeTime);
        FadeSeq.Property(handleColor, HandleColorSave, FFEase.E_SmoothEnd, fadeTime);
        FadeSeq.Sync();
    }
    void FadeOut()
    {
        FadeSeq.ClearSequence();

        var backgroundColor = RefBackgroundColor();
        var fillColor = RefFillColor();
        var handleColor = RefHandleColor();

        backgroundColor.Setter(BackgroundColorSave);
        fillColor.Setter(FillColorSave);
        handleColor.Setter(HandleColorSave);

        FadeSeq.Property(backgroundColor, BackgroundColorSave.MakeClear(), FFEase.E_SmoothStart, fadeTime * 0.12f);
        FadeSeq.Property(fillColor, FillColorSave.MakeClear(), FFEase.E_SmoothStart, fadeTime * 0.12f);
        FadeSeq.Property(handleColor, HandleColorSave.MakeClear(), FFEase.E_SmoothStart, fadeTime * 0.12f);
        FadeSeq.Sync();
        FadeSeq.Call(ObjectActive, false);
    }
}
