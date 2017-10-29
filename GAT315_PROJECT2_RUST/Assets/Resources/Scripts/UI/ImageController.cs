using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageController : UIBase {

    Color ImageColorSave;
    FFAction.ActionSequence FadeSeq;

    private void Awake()
    {
        // Save color
        ImageColorSave = RefImageColor().Val;
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



    public FFRef<Color> RefImageColor()
    {
        return new FFRef<Color>(() => transform.GetComponent<UnityEngine.UI.Image>().color, (v) => { transform.GetComponent<UnityEngine.UI.Image>().color = v; });
    }

    float fadeTime = 0.9f;
    void FadeIn()
    {
        FadeSeq.ClearSequence();
        var imageColor = RefImageColor();

        imageColor.Setter(ImageColorSave.MakeClear());

        ObjectActive(true);
        FadeSeq.Property(imageColor, ImageColorSave, FFEase.E_SmoothEnd, fadeTime);
        FadeSeq.Sync();
    }
    void FadeOut()
    {
        FadeSeq.ClearSequence();
        var imageColor = RefImageColor();
        imageColor.Setter(ImageColorSave);

        FadeSeq.Property(imageColor, ImageColorSave.MakeClear(), FFEase.E_SmoothStart, fadeTime * 0.1f);
        FadeSeq.Sync();
        FadeSeq.Call(ObjectActive, false);
    }
}
