using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextController : UIBase {


    Color TextColorSave;
    FFAction.ActionSequence FadeSeq;

    private void Awake()
    {
        // Save color
        TextColorSave = RefTextColor().Val;
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



    public FFRef<Color> RefTextColor()
    {
        return new FFRef<Color>(() => transform.GetComponent<UnityEngine.UI.Text>().color, (v) => { transform.GetComponent<UnityEngine.UI.Text>().color = v; });
    }

    float fadeTime = 0.9f;
    void FadeIn()
    {
        FadeSeq.ClearSequence();
        var textColor = RefTextColor();

        textColor.Setter(TextColorSave.MakeClear());

        ObjectActive(true);
        FadeSeq.Property(textColor, TextColorSave, FFEase.E_SmoothEnd, fadeTime);
        FadeSeq.Sync();
    }
    void FadeOut()
    {
        FadeSeq.ClearSequence();
        var textColor = RefTextColor();
        textColor.Setter(TextColorSave);

        FadeSeq.Property(textColor, TextColorSave.MakeClear(), FFEase.E_SmoothStart, fadeTime * 0.1f);
        FadeSeq.Sync();
        FadeSeq.Call(ObjectActive, false);
    }

}
