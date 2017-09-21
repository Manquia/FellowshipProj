using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechController : FFComponent
{
    #region FFRef
    
    public FFRef<Color> BubbleColor()
    {
        return new FFRef<Color>(() => BubbleSprite().color, (v) => { transform.Find("Bubble").GetComponent<SpriteRenderer>().color = v; });
    }
    public FFRef<Color> TextColor()
    {
        return new FFRef<Color>(() => GetDialogText().color, (v) => { GetDialogText().color = v; });
    }

    #endregion


    public SpriteRenderer BubbleSprite()
    {
        return transform.Find("Bubble").GetComponent<SpriteRenderer>();
    }
    public TextMesh GetDialogText()
    {
        return transform.Find("Text").GetComponent<TextMesh>();
    }

}
